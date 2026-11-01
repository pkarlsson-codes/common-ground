using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Marten;
using Marten.Pagination;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Returns a paginated list of purchase workflows with optional filters.</summary>
public sealed record GetPurchaseWorkflows(
    PageSpec Page,
    GroundLeaseTemplateId? GroundLeaseTemplateId = null,
    string? Phase = null,
    string? Search = null,
    string? HomeId = null,
    string? HouseholdId = null);

/// <summary>A purchase workflow list item and the label of the home being purchased.</summary>
public sealed record PurchaseWorkflowListItem(PurchaseWorkflowListItemView Workflow, string? HomeLabel);

/// <summary>Handles <see cref="GetPurchaseWorkflows"/>.</summary>
public static class GetPurchaseWorkflowsHandler
{
    /// <summary>Queries purchase workflows with optional filters and returns a paginated result.</summary>
    public static async Task<PaginatedResponse<PurchaseWorkflowListItem>> Handle(
        GetPurchaseWorkflows request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<PurchaseWorkflowListItemView> query = session.Query<PurchaseWorkflowListItemView>();

        if (request.GroundLeaseTemplateId is not null)
        {
            query = query.Where(v => v.GroundLeaseTemplateId == request.GroundLeaseTemplateId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.HomeId))
        {
            query = query.Where(v => v.HomeId == request.HomeId);
        }

        if (!string.IsNullOrWhiteSpace(request.HouseholdId))
        {
            query = query.Where(v => v.HouseholdId == request.HouseholdId);
        }

        if (!string.IsNullOrWhiteSpace(request.Phase))
        {
            query = query.Where(v => v.Phase == request.Phase);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search;
            query = query.Where(v => v.BuyerFullName.Contains(s, StringComparison.OrdinalIgnoreCase));
        }

        var page = await query
            .OrderByDescending(v => v.CreatedAt)
            .ToPagedListAsync(request.Page.Number, request.Page.Size, cancellationToken)
            .ConfigureAwait(false);

        var homeIds = page.Select(v => v.HomeId).Distinct().ToList();
        var labels = (await session.LoadManyAsync<HomeLabelView>(cancellationToken, homeIds).ConfigureAwait(false))
            .ToDictionary(l => l.Id, l => l.Label);

        return new PaginatedResponse<PurchaseWorkflowListItem>
        {
            Items = [.. page.Select(v => new PurchaseWorkflowListItem(v, labels.GetValueOrDefault(v.HomeId)))],
            TotalCount = (int)page.TotalItemCount,
            PageSize = (int)page.PageSize,
            PageNumber = (int)page.PageNumber,
            TotalPages = (int)page.PageCount,
        };
    }
}
