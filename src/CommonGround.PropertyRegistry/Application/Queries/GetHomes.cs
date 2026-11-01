using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Marten;
using Marten.Pagination;

namespace CommonGround.PropertyRegistry.Application.Queries;

/// <summary>Returns a paginated list of homes with optional filters.</summary>
/// <param name="Page">Pagination spec.</param>
/// <param name="LandParcelIds">Optional land parcels to filter by.</param>
/// <param name="Statuses">Optional statuses to filter by.</param>
public sealed record GetHomes(
    PageSpec Page,
    IReadOnlyList<LandParcelId>? LandParcelIds = null,
    IReadOnlyList<HomeStatus>? Statuses = null);

/// <summary>A home list item and the label staff know the home by.</summary>
public sealed record HomeListItem(HomeListItemView Home, string Label);

/// <summary>Handles <see cref="GetHomes"/>.</summary>
public static class GetHomesHandler
{
    /// <summary>Returns a paginated list of homes, each labelled with its parcel's address.</summary>
    public static async Task<PaginatedResponse<HomeListItem>> Handle(
        GetHomes request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<HomeListItemView> query = session.Query<HomeListItemView>();

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

        var parcelIds = page.Select(h => h.LandParcelId).Distinct().ToList();
        var parcels = await session.LoadManyAsync<LandParcelView>(cancellationToken, parcelIds).ConfigureAwait(false);
        var addresses = parcels.ToDictionary(p => p.Id, p => p.Address);

        return new PaginatedResponse<HomeListItem>
        {
            Items = [.. page.Select(h => new HomeListItem(
                h, HomeLabel.Format(addresses.GetValueOrDefault(h.LandParcelId, string.Empty), h.UnitDesignation)))],
            TotalCount = (int)page.TotalItemCount,
            PageSize = (int)page.PageSize,
            PageNumber = (int)page.PageNumber,
            TotalPages = (int)page.PageCount,
        };
    }
}
