using System.Globalization;
using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Marten;
using Marten.Pagination;

namespace CommonGround.PropertyRegistry.Application.Queries;

/// <summary>Returns a paginated list of development projects with optional filters.</summary>
/// <param name="Page">Pagination spec (page number + size).</param>
/// <param name="Search">Optional text search across description or status.</param>
public sealed record GetDevelopmentProjects(
    PageSpec Page,
    string? Search = null);
    
/// <summary>Handles <see cref="GetDevelopmentProjects"/>.</summary>
public static class GetDevelopmentProjectsHandler
{
    /// <summary>Returns a paginated list of <see cref="DevelopmentProjectListItemView"/>.</summary>
    public static async Task<PaginatedResponse<DevelopmentProjectListItemView>> Handle(
        GetDevelopmentProjects request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<DevelopmentProjectListItemView> query = session.Query<DevelopmentProjectListItemView>();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower(CultureInfo.InvariantCulture);
            query = query.Where(p =>
                p.Description != null && p.Description.Contains(s, StringComparison.OrdinalIgnoreCase));
        }

        var page = await query
            .ToPagedListAsync(request.Page.Number, request.Page.Size, cancellationToken)
            .ConfigureAwait(false);

        return new PaginatedResponse<DevelopmentProjectListItemView>
        {
            Items = [.. page],
            TotalCount = (int)page.TotalItemCount,
            PageSize = (int)page.PageSize,
            PageNumber = (int)page.PageNumber,
            TotalPages = (int)page.PageCount,
        };
    }
}
