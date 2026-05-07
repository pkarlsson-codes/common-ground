using CommonGround.Stewardship.Infrastructure.Persistence;
using Marten;

namespace CommonGround.Stewardship.Application.Queries;

/// <summary>
/// Query to get a single stewardship case.
/// </summary>
/// <param name="StewardshipCaseId">The ID of the stewardship case to retrieve.</param>
public sealed record GetStewardshipCaseQuery(string StewardshipCaseId);

/// <summary>
/// Query to get all stewardship cases for a specific ground lease.
/// </summary>
/// <param name="GroundLeaseId">The ID of the ground lease.</param>
public sealed record GetStewardshipCasesByGroundLeaseQuery(string GroundLeaseId);

/// <summary>
/// Query to get all active stewardship cases across all ground leases.
/// </summary>
public sealed record GetAllStewardshipCasesQuery;

/// <summary>Handles <see cref="GetStewardshipCaseQuery"/>.</summary>
public static class GetStewardshipCaseHandler
{
    /// <summary>Returns the <see cref="StewardshipCaseView"/> for the given ID, or null if not found.</summary>
    public static async Task<StewardshipCaseView?> Handle(GetStewardshipCaseQuery query, IQuerySession session, CancellationToken ct)
        => await session.LoadAsync<StewardshipCaseView>(query.StewardshipCaseId, ct).ConfigureAwait(false);
}

/// <summary>Handles <see cref="GetStewardshipCasesByGroundLeaseQuery"/>.</summary>
public static class GetStewardshipCasesByGroundLeaseHandler
{
    /// <summary>Returns all stewardship cases for the given ground lease.</summary>
    public static async Task<IReadOnlyList<StewardshipCaseView>> Handle(GetStewardshipCasesByGroundLeaseQuery query, IQuerySession session, CancellationToken ct)
        => await session.Query<StewardshipCaseView>()
            .Where(x => x.GroundLeaseId == query.GroundLeaseId)
            .ToListAsync(ct)
            .ConfigureAwait(false);
}

/// <summary>Handles <see cref="GetAllStewardshipCasesQuery"/>.</summary>
public static class GetAllStewardshipCasesHandler
{
    /// <summary>Returns all stewardship cases whose status is Active (i.e. not yet closed).</summary>
    public static async Task<IReadOnlyList<StewardshipCaseView>> Handle(GetAllStewardshipCasesQuery query, IQuerySession session, CancellationToken ct)
        => await session.Query<StewardshipCaseView>()
            .Where(x => x.Status == "Active")
            .ToListAsync(ct)
            .ConfigureAwait(false);
}
