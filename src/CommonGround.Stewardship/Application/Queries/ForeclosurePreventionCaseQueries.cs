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

/// <summary>Handles <see cref="GetForeclosurePreventionCaseQuery"/>.</summary>
public static class GetForeclosurePreventionCaseHandler
{
    /// <summary>Returns the <see cref="ForeclosurePreventionCaseView"/> for the given ID, or null if not found.</summary>
    public static async Task<ForeclosurePreventionCaseView?> Handle(GetForeclosurePreventionCaseQuery query, IQuerySession session, CancellationToken ct)
        => await session.LoadAsync<ForeclosurePreventionCaseView>(query.ForeclosurePreventionCaseId, ct).ConfigureAwait(false);
}

/// <summary>Handles <see cref="GetForeclosurePreventionCasesByGroundLeaseQuery"/>.</summary>
public static class GetForeclosurePreventionCasesByGroundLeaseHandler
{
    /// <summary>Returns all foreclosure prevention cases for the given ground lease.</summary>
    public static async Task<IReadOnlyList<ForeclosurePreventionCaseView>> Handle(GetForeclosurePreventionCasesByGroundLeaseQuery query, IQuerySession session, CancellationToken ct)
        => await session.Query<ForeclosurePreventionCaseView>()
            .Where(x => x.GroundLeaseId == query.GroundLeaseId)
            .ToListAsync(ct)
            .ConfigureAwait(false);
}

/// <summary>Handles <see cref="GetAllForeclosurePreventionCasesQuery"/>.</summary>
public static class GetAllForeclosurePreventionCasesHandler
{
    /// <summary>Returns all foreclosure prevention cases not in the Resolved terminal state (Opened + Active).</summary>
    public static async Task<IReadOnlyList<ForeclosurePreventionCaseView>> Handle(GetAllForeclosurePreventionCasesQuery query, IQuerySession session, CancellationToken ct)
        => await session.Query<ForeclosurePreventionCaseView>()
            .Where(x => x.Status != "Resolved")
            .ToListAsync(ct)
            .ConfigureAwait(false);
}
