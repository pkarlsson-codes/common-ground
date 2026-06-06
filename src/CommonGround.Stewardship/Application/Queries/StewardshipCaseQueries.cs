using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using CommonGround.Stewardship.Infrastructure.Persistence;
using Marten;
using Marten.Pagination;

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
/// <param name="Page">Pagination spec.</param>
public sealed record GetStewardshipCasesByGroundLeaseQuery(string GroundLeaseId, PageSpec Page);

/// <summary>
/// Query to get all active stewardship cases across all ground leases.
/// </summary>
/// <param name="Page">Pagination spec.</param>
public sealed record GetAllStewardshipCasesQuery(PageSpec Page);

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
    /// <summary>Returns paginated stewardship cases for the given ground lease.</summary>
    public static async Task<PaginatedResponse<StewardshipCaseView>> Handle(GetStewardshipCasesByGroundLeaseQuery query, IQuerySession session, CancellationToken ct)
    {
        var page = await session.Query<StewardshipCaseView>()
            .Where(x => x.GroundLeaseId == query.GroundLeaseId)
            .ToPagedListAsync(query.Page.Number, query.Page.Size, ct)
            .ConfigureAwait(false);

        return new PaginatedResponse<StewardshipCaseView>
        {
            Items = [.. page],
            TotalCount = (int)page.TotalItemCount,
            PageSize = (int)page.PageSize,
            PageNumber = (int)page.PageNumber,
            TotalPages = (int)page.PageCount,
        };
    }
}

/// <summary>Handles <see cref="GetAllStewardshipCasesQuery"/>.</summary>
public static class GetAllStewardshipCasesHandler
{
    /// <summary>Returns paginated stewardship cases whose status is Active (i.e. not yet closed).</summary>
    public static async Task<PaginatedResponse<StewardshipCaseView>> Handle(GetAllStewardshipCasesQuery query, IQuerySession session, CancellationToken ct)
    {
        var page = await session.Query<StewardshipCaseView>()
            .Where(x => x.Status == "Active")
            .ToPagedListAsync(query.Page.Number, query.Page.Size, ct)
            .ConfigureAwait(false);

        return new PaginatedResponse<StewardshipCaseView>
        {
            Items = [.. page],
            TotalCount = (int)page.TotalItemCount,
            PageSize = (int)page.PageSize,
            PageNumber = (int)page.PageNumber,
            TotalPages = (int)page.PageCount,
        };
    }
}
