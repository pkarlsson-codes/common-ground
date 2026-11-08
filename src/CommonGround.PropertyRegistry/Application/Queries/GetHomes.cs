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
/// <param name="Occupancy">Optional occupancies to filter by.</param>
/// <param name="SaleStages">Optional sale stages to filter by.</param>
public sealed record GetHomes(
    PageSpec Page,
    IReadOnlyList<LandParcelId>? LandParcelIds = null,
    IReadOnlyList<HomeOccupancy>? Occupancy = null,
    IReadOnlyList<HomeSaleStage>? SaleStages = null);

/// <summary>A home list item, the label staff know the home by, and its occupancy and sale stage display names.</summary>
public sealed record HomeListItem(HomeListItemView Home, string Label, string Occupancy, string SaleStage);

/// <summary>Handles <see cref="GetHomes"/>.</summary>
public static class GetHomesHandler
{
    /// <summary>Returns a paginated list of homes, each labelled with its parcel's address and showing its occupancy and sale stage.</summary>
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

        if (request.Occupancy is { Count: > 0 } || request.SaleStages is { Count: > 0 })
        {
            // A home with no availability yet is vacant and offered, so the homes to keep are found by
            // excluding every home whose availability does not match.
            var occupancy = (request.Occupancy ?? []).Select(o => o.DisplayName).ToList();
            var saleStages = (request.SaleStages ?? []).Select(s => s.DisplayName).ToList();
            var defaultMatches = (occupancy.Count == 0 || occupancy.Contains(HomeOccupancy.Vacant.DisplayName))
                && (saleStages.Count == 0 || saleStages.Contains(HomeSaleStage.Offered.DisplayName));

            var matching = session.Query<HomeAvailabilityView>();
            if (defaultMatches)
            {
                var excluded = await matching
                    .Where(v => (occupancy.Count > 0 && !occupancy.Contains(v.Occupancy))
                        || (saleStages.Count > 0 && !saleStages.Contains(v.SaleStage)))
                    .Select(v => v.Id)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
                query = query.Where(h => !excluded.Contains(h.Id));
            }
            else
            {
                var included = await matching
                    .Where(v => (occupancy.Count == 0 || occupancy.Contains(v.Occupancy))
                        && (saleStages.Count == 0 || saleStages.Contains(v.SaleStage)))
                    .Select(v => v.Id)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
                query = query.Where(h => included.Contains(h.Id));
            }
        }

        var page = await query
            .ToPagedListAsync(request.Page.Number, request.Page.Size, cancellationToken)
            .ConfigureAwait(false);

        var parcelIds = page.Select(h => h.LandParcelId).Distinct().ToList();
        var parcels = await session.LoadManyAsync<LandParcelView>(cancellationToken, parcelIds).ConfigureAwait(false);
        var addresses = parcels.ToDictionary(p => p.Id, p => p.Address);
        var availabilities = await session.LoadManyAsync<HomeAvailabilityView>(cancellationToken, page.Select(h => h.Id)).ConfigureAwait(false);
        var byId = availabilities.ToDictionary(a => a.Id);

        return new PaginatedResponse<HomeListItem>
        {
            Items = [.. page.Select(h =>
            {
                // A home with no availability yet is available: vacant and offered.
                var availability = byId.GetValueOrDefault(h.Id) ?? new HomeAvailabilityView { Id = h.Id };
                return new HomeListItem(
                    h,
                    HomeLabel.Format(addresses.GetValueOrDefault(h.LandParcelId, string.Empty), h.UnitDesignation),
                    availability.Occupancy,
                    availability.SaleStage);
            })],
            TotalCount = (int)page.TotalItemCount,
            PageSize = (int)page.PageSize,
            PageNumber = (int)page.PageNumber,
            TotalPages = (int)page.PageCount,
        };
    }
}
