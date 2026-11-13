using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.SharedKernel.Domain.Services;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>
/// Returns the active leases with no resale in progress, each with its last resale price statement and when the next
/// is due, soonest due first, with the trust's today and letter sign-off. By default only leases due today or
/// earlier. Not paged: the yearly round prints every lease due.
/// </summary>
/// <param name="IncludeAll">Whether to list leases not due yet too.</param>
/// <param name="Search">Matches the homeowner's name or the home's label, case-insensitive.</param>
public sealed record GetResalePriceStatementsDue(bool IncludeAll = false, string? Search = null);

/// <summary>The leases on the yearly statement cycle.</summary>
/// <param name="Today">The trust's today, the default statement date.</param>
/// <param name="LetterSignOff">The trust's sign-off; empty for none.</param>
/// <param name="Leases">The leases, soonest due first.</param>
public sealed record ResalePriceStatementsDue(DateOnly Today, string LetterSignOff, IReadOnlyList<ResalePriceStatementSchedule> Leases);

/// <summary>A lease on the yearly statement cycle.</summary>
/// <param name="Lease">The lease.</param>
/// <param name="HomeLabel">The label of the lease's home, if it has arrived.</param>
/// <param name="LastStatementDate">The latest statement's date, or null when none was sent.</param>
/// <param name="NextDueOn">When the next statement is due.</param>
public sealed record ResalePriceStatementSchedule(
    GroundLeaseView Lease, string? HomeLabel, DateOnly? LastStatementDate, DateOnly NextDueOn)
{
    /// <summary>
    /// The lease's schedule, or null for a lease off the cycle: an ended one, or one with a resale in progress,
    /// whose resale appraisal sets its price.
    /// </summary>
    public static ResalePriceStatementSchedule? For(GroundLeaseView lease, string? homeLabel)
    {
        ArgumentNullException.ThrowIfNull(lease);

        if (lease.Status != GroundLeaseStatus.Active.DisplayName || lease.OpenResaleId is not null)
            return null;

        var last = lease.ResalePriceStatements.Select(s => (DateOnly?)s.StatementDate).Max();
        return new ResalePriceStatementSchedule(lease, homeLabel, last, ResalePriceStatementPrice.NextDueOn(lease.LeaseStartDate, last));
    }

    /// <summary>
    /// Loads every lease on the cycle, soonest due first, then homeowner name. A trust has hundreds of leases, so
    /// they are filtered in memory.
    /// </summary>
    public static async Task<List<ResalePriceStatementSchedule>> LoadAllAsync(IQuerySession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);

        var active = GroundLeaseStatus.Active.DisplayName;
        var leases = await session.Query<GroundLeaseView>()
            .Where(l => l.Status == active && l.OpenResaleId == null)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var labels = (await session.LoadManyAsync<HomeLabelView>(cancellationToken, leases.Select(l => l.HomeId).Distinct())
                .ConfigureAwait(false))
            .ToDictionary(l => l.Id, l => l.Label);

        return [.. leases
            .Select(l => For(l, labels.GetValueOrDefault(l.HomeId)))
            .OfType<ResalePriceStatementSchedule>()
            .OrderBy(s => s.NextDueOn)
            .ThenBy(s => s.Lease.HomeownerFullName, StringComparer.OrdinalIgnoreCase)];
    }
}

/// <summary>Handles <see cref="GetResalePriceStatementsDue"/>.</summary>
public static class GetResalePriceStatementsDueHandler
{
    /// <summary>Filters the trust's leases on the cycle as of its today.</summary>
    public static async Task<ResalePriceStatementsDue> Handle(
        GetResalePriceStatementsDue request,
        IQuerySession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);
        var policy = await GetLeaseFeeNoticePolicyHandler.LoadAsync(session, cancellationToken).ConfigureAwait(false);
        var all = await ResalePriceStatementSchedule.LoadAllAsync(session, cancellationToken).ConfigureAwait(false);
        var search = request.Search?.Trim();

        return new ResalePriceStatementsDue(
            today,
            policy.LetterSignOff,
            [.. all
                .Where(s => request.IncludeAll || s.NextDueOn <= today)
                .Where(s => string.IsNullOrEmpty(search)
                    || (s.Lease.HomeownerFullName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (s.HomeLabel?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))]);
    }
}
