using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using CommonGround.Stewardship.Infrastructure.Persistence;
using Marten;
using Marten.Pagination;

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
/// <param name="Page">Pagination spec.</param>
public sealed record GetForeclosurePreventionCasesByGroundLeaseQuery(string GroundLeaseId, PageSpec Page);

/// <summary>
/// Query to get all non-resolved foreclosure prevention cases across all ground leases.
/// </summary>
/// <param name="Page">Pagination spec.</param>
public sealed record GetAllForeclosurePreventionCasesQuery(PageSpec Page);

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
    /// <summary>Returns paginated foreclosure prevention cases for the given ground lease.</summary>
    public static async Task<PaginatedResponse<ForeclosurePreventionCaseView>> Handle(GetForeclosurePreventionCasesByGroundLeaseQuery query, IQuerySession session, CancellationToken ct)
    {
        var page = await session.Query<ForeclosurePreventionCaseView>()
            .Where(x => x.GroundLeaseId == query.GroundLeaseId)
            .OrderByDescending(x => x.OpenedDate)
            .ThenByDescending(x => x.Id)
            .ToPagedListAsync(query.Page.Number, query.Page.Size, ct)
            .ConfigureAwait(false);

        return new PaginatedResponse<ForeclosurePreventionCaseView>
        {
            Items = [.. page],
            TotalCount = (int)page.TotalItemCount,
            PageSize = (int)page.PageSize,
            PageNumber = (int)page.PageNumber,
            TotalPages = (int)page.PageCount,
        };
    }
}

/// <summary>Handles <see cref="GetAllForeclosurePreventionCasesQuery"/>.</summary>
public static class GetAllForeclosurePreventionCasesHandler
{
    /// <summary>Returns paginated foreclosure prevention cases not in the Resolved terminal state (Opened + Active), newest first.</summary>
    public static async Task<PaginatedResponse<ForeclosurePreventionCaseView>> Handle(GetAllForeclosurePreventionCasesQuery query, IQuerySession session, CancellationToken ct)
    {
        var page = await session.Query<ForeclosurePreventionCaseView>()
            .Where(x => x.Status != "Resolved")
            .OrderByDescending(x => x.OpenedDate)
            .ThenByDescending(x => x.Id)
            .ToPagedListAsync(query.Page.Number, query.Page.Size, ct)
            .ConfigureAwait(false);

        return new PaginatedResponse<ForeclosurePreventionCaseView>
        {
            Items = [.. page],
            TotalCount = (int)page.TotalItemCount,
            PageSize = (int)page.PageSize,
            PageNumber = (int)page.PageNumber,
            TotalPages = (int)page.PageCount,
        };
    }
}
