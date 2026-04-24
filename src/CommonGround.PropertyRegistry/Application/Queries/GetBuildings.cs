using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.Buildings;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Marten;
using Marten.Pagination;

namespace CommonGround.PropertyRegistry.Application.Queries;

/// <summary>Returns a paginated list of buildings with optional filters.</summary>
/// <param name="Page">Pagination spec.</param>
/// <param name="LandParcelIds">Optional land parcels to filter by.</param>
/// <param name="Statuses">Optional statuses to filter by.</param>
public sealed record GetBuildings(
    PageSpec Page,
    IReadOnlyList<LandParcelId>? LandParcelIds = null,
    IReadOnlyList<BuildingStatus>? Statuses = null);

/// <summary>Handles <see cref="GetBuildings"/>.</summary>
public static class GetBuildingsHandler
{
    /// <summary>Returns a paginated list of <see cref="BuildingListItemView"/>.</summary>
    public static async Task<PaginatedResponse<BuildingListItemView>> Handle(
        GetBuildings request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<BuildingListItemView> query = session.Query<BuildingListItemView>();

        if (request.LandParcelIds is { Count: > 0 })
        {
            var ids = request.LandParcelIds.Select(id => id.Value).ToList();
            query = query.Where(s => ids.Contains(s.LandParcelId));
        }

        if (request.Statuses is { Count: > 0 })
        {
            var statusNames = request.Statuses.Select(s => s.DisplayName).ToList();
            query = query.Where(s => statusNames.Contains(s.Status));
        }

        var page = await query
            .ToPagedListAsync(request.Page.Number, request.Page.Size, cancellationToken)
            .ConfigureAwait(false);

        return new PaginatedResponse<BuildingListItemView>
        {
            Items = [.. page],
            TotalCount = (int)page.TotalItemCount,
            PageSize = (int)page.PageSize,
            PageNumber = (int)page.PageNumber,
            TotalPages = (int)page.PageCount,
        };
    }
}
