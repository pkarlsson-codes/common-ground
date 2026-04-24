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
/// Handles queries for stewardship cases.
/// </summary>
public sealed class StewardshipCaseQueryHandler(IQuerySession session)
{
    /// <summary>
    /// Handles the GetStewardshipCaseQuery.
    /// </summary>
    public async Task<StewardshipCaseView?> Handle(GetStewardshipCaseQuery query, CancellationToken ct)
        => await session.LoadAsync<StewardshipCaseView>(query.StewardshipCaseId, ct).ConfigureAwait(false);

    /// <summary>
    /// Handles the GetStewardshipCasesByGroundLeaseQuery.
    /// </summary>
    public async Task<IReadOnlyList<StewardshipCaseView>> Handle(GetStewardshipCasesByGroundLeaseQuery query, CancellationToken ct)
        => await session.Query<StewardshipCaseView>()
            .Where(x => x.GroundLeaseId == query.GroundLeaseId)
            .ToListAsync(ct)
            .ConfigureAwait(false);
}
