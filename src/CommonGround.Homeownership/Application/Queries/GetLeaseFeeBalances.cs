using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Works out a lease's fee statement from its read model.</summary>
public static class LeaseFeeStatements
{
    /// <summary>The statement for a lease as of a day.</summary>
    public static LeaseFeeStatement For(GroundLeaseView view, DateOnly asOf)
    {
        ArgumentNullException.ThrowIfNull(view);

        return LeaseFeeStatement.Calculate(
            TermsOf(view),
            view.LeaseStartDate,
            view.EndedOn,
            PaymentsOf(view),
            asOf,
            view.LeaseFeeInterestWaivers.Select(w => new WaivedInterest(w.WaivedOn, w.Amount)));
    }

    /// <summary>
    /// Where the lease's last notice of default stands as of a day, or null. Mirrors
    /// <see cref="GroundLease.DefaultCureAsOf"/>.
    /// </summary>
    public static DefaultCure? DefaultCureOf(GroundLeaseView view, DateOnly asOf)
    {
        ArgumentNullException.ThrowIfNull(view);

        var notice = view.LeaseFeeNotices
            .Where(n => n.Level == LeaseFeeNoticeLevel.Default.DisplayName && n.CureDays is not null && n.SentOn <= asOf)
            .MaxBy(n => n.SentOn);

        return notice is null
            ? null
            : DefaultCure.Calculate(
                notice.SentOn,
                notice.BalanceDue,
                DefaultCurePeriod.Create(notice.CureDays!.Value, notice.CureExtensionDays ?? 0),
                PaymentsOf(view),
                asOf);
    }

    private static IEnumerable<ReceivedPayment> PaymentsOf(GroundLeaseView view) =>
        view.LeaseFeePayments.Where(p => p.VoidReason is null).Select(p => new ReceivedPayment(p.ReceivedDate, p.Amount));

    /// <summary>The lease's fee and interest terms.</summary>
    public static LeaseFeeTerms TermsOf(GroundLeaseView view)
    {
        ArgumentNullException.ThrowIfNull(view);

        return LeaseFeeTerms.Create(view.LandUseFee, view.RepairReserveFee, view.LateInterestRate, view.InterestWaiverDays);
    }

    /// <summary>
    /// The late notice a lease needs as of a day, or null. Mirrors <see cref="GroundLease.LeaseFeeNoticeDueAsOf"/>:
    /// only an active lease needs one.
    /// </summary>
    public static LeaseFeeNoticeLevel? NoticeDue(GroundLeaseView view, LeaseFeeStatement statement, LeaseFeeNoticeSchedule schedule)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(schedule);

        return view.Status != GroundLeaseStatus.Active.DisplayName
            ? null
            : schedule.NoticeDue(
                statement,
                view.LeaseFeeNotices.Select(n => new LeaseFeeNoticeSent(LeaseFeeNoticeLevel.FromDisplayName(n.Level), n.SentOn)));
    }

    /// <summary>The last notice sent since the lease's current arrears began, or null.</summary>
    public static LeaseFeeNoticeEntry? LastNoticeInArrears(GroundLeaseView view, LeaseFeeStatement statement)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(statement);

        return statement.ArrearsSince is { } since
            ? view.LeaseFeeNotices.Where(n => n.SentOn >= since).MaxBy(n => n.SentOn)
            : null;
    }
}

/// <summary>
/// Returns leases with what they owe in lease fees as of the trust's today, most days overdue first, then largest
/// balance. By default only leases with a balance due; with <paramref name="IncludeAll"/>, every active lease and
/// every ended lease with a balance; with <paramref name="NoticeDueOnly"/>, only leases with a late notice due.
/// </summary>
/// <param name="Page">The page to return.</param>
/// <param name="IncludeAll">Whether to list leases with nothing due too.</param>
/// <param name="Search">Matches the homeowner's name or the home's label, case-insensitive.</param>
/// <param name="NoticeDueOnly">Whether to list only leases with a late notice due; wins over <paramref name="IncludeAll"/>.</param>
public sealed record GetLeaseFeeBalances(PageSpec Page, bool IncludeAll = false, string? Search = null, bool NoticeDueOnly = false);

/// <summary>A lease, its home's label, its fee statement, and its late notices.</summary>
/// <param name="Lease">The lease.</param>
/// <param name="HomeLabel">The label of the lease's home, if it has arrived.</param>
/// <param name="Statement">What the lease owes as of today.</param>
/// <param name="NoticeDue">The late notice it needs now, or null.</param>
/// <param name="LastNotice">The last notice sent since its arrears began, or null.</param>
/// <param name="DefaultCure">Where its last notice of default stands, or null.</param>
public sealed record LeaseFeeBalance(
    GroundLeaseView Lease,
    string? HomeLabel,
    LeaseFeeStatement Statement,
    LeaseFeeNoticeLevel? NoticeDue,
    LeaseFeeNoticeEntry? LastNotice,
    DefaultCure? DefaultCure);

/// <summary>Works out every lease's balance and notice due.</summary>
public static class LeaseFeeBalances
{
    /// <summary>
    /// Loads every lease of the session's trust and works out its statement in memory. The balance depends on today,
    /// so no stored view can hold it; a trust has hundreds of leases, not millions. Most days overdue first, then
    /// largest balance, then homeowner name.
    /// </summary>
    public static async Task<List<LeaseFeeBalance>> LoadAsync(
        IQuerySession session,
        DateOnly today,
        LeaseFeeNoticeSchedule schedule,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(schedule);

        var leases = await session.Query<GroundLeaseView>().ToListAsync(cancellationToken).ConfigureAwait(false);
        var labels = (await session.LoadManyAsync<HomeLabelView>(cancellationToken, leases.Select(l => l.HomeId).Distinct())
                .ConfigureAwait(false))
            .ToDictionary(l => l.Id, l => l.Label);

        return [.. leases
            .Select(l =>
            {
                var statement = LeaseFeeStatements.For(l, today);
                return new LeaseFeeBalance(
                    l,
                    labels.GetValueOrDefault(l.HomeId),
                    statement,
                    LeaseFeeStatements.NoticeDue(l, statement, schedule),
                    LeaseFeeStatements.LastNoticeInArrears(l, statement),
                    LeaseFeeStatements.DefaultCureOf(l, today));
            })
            .OrderByDescending(b => b.Statement.DaysOverdue)
            .ThenByDescending(b => b.Statement.Balance)
            .ThenBy(b => b.Lease.HomeownerFullName, StringComparer.OrdinalIgnoreCase)];
    }
}

/// <summary>Handles <see cref="GetLeaseFeeBalances"/>.</summary>
public static class GetLeaseFeeBalancesHandler
{
    /// <summary>Filters and pages the trust's lease fee balances.</summary>
    public static async Task<PaginatedResponse<LeaseFeeBalance>> Handle(
        GetLeaseFeeBalances request,
        IQuerySession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(trustClock);

        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);
        var policy = await GetLeaseFeeNoticePolicyHandler.LoadAsync(session, cancellationToken).ConfigureAwait(false);
        var all = await LeaseFeeBalances.LoadAsync(session, today, policy.Schedule, cancellationToken).ConfigureAwait(false);

        var active = GroundLeaseStatus.Active.DisplayName;
        var search = request.Search?.Trim();

        var balances = all
            .Where(b => request.NoticeDueOnly ? b.NoticeDue is not null
                : request.IncludeAll ? b.Lease.Status == active || b.Statement.Balance > 0
                : b.Statement.Balance > 0)
            .Where(b => string.IsNullOrEmpty(search)
                || (b.Lease.HomeownerFullName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (b.HomeLabel?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();

        var size = request.Page.Size;
        var number = request.Page.Number;
        return new PaginatedResponse<LeaseFeeBalance>
        {
            Items = [.. balances.Skip((number - 1) * size).Take(size)],
            TotalCount = balances.Count,
            PageSize = size,
            PageNumber = number,
            TotalPages = (int)Math.Ceiling(balances.Count / (double)size),
        };
    }
}
