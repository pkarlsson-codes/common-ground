using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Marten;
using Marten.Pagination;

namespace CommonGround.PropertyRegistry.Application.Queries;

/// <summary>Returns a paginated list of land parcels with optional filters.</summary>
/// <param name="Page">Pagination spec (page number + size).</param>
/// <param name="Statuses">Optional statuses to filter by.</param>
public sealed record GetLandParcels(
    PageSpec Page,
    IReadOnlyList<ParcelStatus>? Statuses = null);

/// <summary>Handles <see cref="GetLandParcels"/>.</summary>
public static class GetLandParcelsHandler
{
    /// <summary>Returns a paginated list of <see cref="LandParcelListItemView"/>.</summary>
    public static async Task<PaginatedResponse<LandParcelListItemView>> Handle(
        GetLandParcels request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<LandParcelListItemView> query = session.Query<LandParcelListItemView>();

        if (request.Statuses is { Count: > 0 })
        {
            var statusNames = request.Statuses.Select(s => s.DisplayName).ToList();
            query = query.Where(p => statusNames.Contains(p.Status));
        }

        var page = await query
            .ToPagedListAsync(request.Page.Number, request.Page.Size, cancellationToken)
            .ConfigureAwait(false);

        return new PaginatedResponse<LandParcelListItemView>
        {
            Items = [.. page],
            TotalCount = (int)page.TotalItemCount,
            PageSize = (int)page.PageSize,
            PageNumber = (int)page.PageNumber,
            TotalPages = (int)page.PageCount,
        };
    }
}
