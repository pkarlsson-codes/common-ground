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
            LeaseFeeTerms.Create(view.LandUseFee, view.RepairReserveFee),
            view.LeaseStartDate,
            view.EndedOn,
            view.LeaseFeePayments.Where(p => p.VoidReason is null).Select(p => p.Amount),
            asOf);
    }
}

/// <summary>
/// Returns leases with what they owe in lease fees as of the trust's today, most days overdue first, then largest
/// balance. By default only leases with a balance due; with <paramref name="IncludeAll"/>, every active lease and
/// every ended lease with a balance.
/// </summary>
/// <param name="Page">The page to return.</param>
/// <param name="IncludeAll">Whether to list leases with nothing due too.</param>
/// <param name="Search">Matches the homeowner's name or the home's label, case-insensitive.</param>
public sealed record GetLeaseFeeBalances(PageSpec Page, bool IncludeAll = false, string? Search = null);

/// <summary>A lease, its home's label and its fee statement.</summary>
public sealed record LeaseFeeBalance(GroundLeaseView Lease, string? HomeLabel, LeaseFeeStatement Statement);

/// <summary>Handles <see cref="GetLeaseFeeBalances"/>.</summary>
public static class GetLeaseFeeBalancesHandler
{
    /// <summary>
    /// Works out every lease's statement in memory. The balance depends on today, so no stored view can hold it;
    /// a trust has hundreds of leases, not millions.
    /// </summary>
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
        var leases = await session.Query<GroundLeaseView>().ToListAsync(cancellationToken).ConfigureAwait(false);
        var labels = (await session.LoadManyAsync<HomeLabelView>(cancellationToken, leases.Select(l => l.HomeId).Distinct())
                .ConfigureAwait(false))
            .ToDictionary(l => l.Id, l => l.Label);

        var active = GroundLeaseStatus.Active.DisplayName;
        var search = request.Search?.Trim();

        var balances = leases
            .Select(l => new LeaseFeeBalance(l, labels.GetValueOrDefault(l.HomeId), LeaseFeeStatements.For(l, today)))
            .Where(b => request.IncludeAll
                ? b.Lease.Status == active || b.Statement.Balance > 0
                : b.Statement.Balance > 0)
            .Where(b => string.IsNullOrEmpty(search)
                || (b.Lease.HomeownerFullName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (b.HomeLabel?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderByDescending(b => b.Statement.DaysOverdue)
            .ThenByDescending(b => b.Statement.Balance)
            .ThenBy(b => b.Lease.HomeownerFullName, StringComparer.OrdinalIgnoreCase)
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
