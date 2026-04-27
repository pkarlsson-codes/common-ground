using CommonGround.Stewardship.Infrastructure.Persistence;
using Marten;

namespace CommonGround.Stewardship.Application.Queries;

/// <summary>
/// Query to get a single foreclosure prevention case.
/// </summary>
/// <param name="ForeclosurePreventionCaseId">The ID of the foreclosure prevention case to retrieve.</param>
public sealed record GetForeclosurePreventionCaseQuery(string ForeclosurePreventionCaseId);

/// <summary>
/// Query to get all foreclosure prevention cases for a specific ground lease.
/// </summary>
/// <param name="GroundLeaseId">The ID of the ground lease.</param>
public sealed record GetForeclosurePreventionCasesByGroundLeaseQuery(string GroundLeaseId);

/// <summary>
/// Query to get all non-resolved foreclosure prevention cases across all ground leases.
/// </summary>
public sealed record GetAllForeclosurePreventionCasesQuery;

/// <summary>
/// Handles queries for foreclosure prevention cases.
/// </summary>
public sealed class ForeclosurePreventionCaseQueryHandler(IQuerySession session)
{
    /// <summary>
    /// Handles the GetForeclosurePreventionCaseQuery.
    /// </summary>
    public async Task<ForeclosurePreventionCaseView?> Handle(GetForeclosurePreventionCaseQuery query, CancellationToken ct)
        => await session.LoadAsync<ForeclosurePreventionCaseView>(query.ForeclosurePreventionCaseId, ct).ConfigureAwait(false);

    /// <summary>
    /// Handles the GetForeclosurePreventionCasesByGroundLeaseQuery.
    /// </summary>
    public async Task<IReadOnlyList<ForeclosurePreventionCaseView>> Handle(GetForeclosurePreventionCasesByGroundLeaseQuery query, CancellationToken ct)
        => await session.Query<ForeclosurePreventionCaseView>()
            .Where(x => x.GroundLeaseId == query.GroundLeaseId)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    /// <summary>
    /// Handles the GetAllForeclosurePreventionCasesQuery.
    /// Returns all cases that are not in the Resolved terminal state (Opened + Active).
    /// </summary>
    public async Task<IReadOnlyList<ForeclosurePreventionCaseView>> Handle(GetAllForeclosurePreventionCasesQuery query, CancellationToken ct)
        => await session.Query<ForeclosurePreventionCaseView>()
            .Where(x => x.Status != "Resolved")
            .ToListAsync(ct)
            .ConfigureAwait(false);
}
