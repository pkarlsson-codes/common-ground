using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Marten;
using Marten.Pagination;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Returns a paginated list of ground lease templates, ordered by name, with optional filters.</summary>
public sealed record GetGroundLeaseTemplates(
    PageSpec Page,
    string? Search = null,
    string? Status = null);

/// <summary>Handles <see cref="GetGroundLeaseTemplates"/>.</summary>
public static class GetGroundLeaseTemplatesHandler
{
    /// <summary>Queries ground lease templates with optional filters and returns a paginated result.</summary>
    public static async Task<PaginatedResponse<GroundLeaseTemplateListItemView>> Handle(
        GetGroundLeaseTemplates request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<GroundLeaseTemplateListItemView> query = session.Query<GroundLeaseTemplateListItemView>();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search;
            query = query.Where(v => v.TemplateName.Contains(s, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status;
            query = query.Where(v => v.Status == status);
        }

        var page = await query
            .OrderBy(v => v.TemplateName)
            .ToPagedListAsync(request.Page.Number, request.Page.Size, cancellationToken)
            .ConfigureAwait(false);

        return new PaginatedResponse<GroundLeaseTemplateListItemView>
        {
            Items = [.. page],
            TotalCount = (int)page.TotalItemCount,
            PageSize = (int)page.PageSize,
            PageNumber = (int)page.PageNumber,
            TotalPages = (int)page.PageCount,
        };
    }
}
