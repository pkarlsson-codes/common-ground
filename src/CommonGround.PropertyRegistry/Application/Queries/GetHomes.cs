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

/// <summary>A home list item, the label staff know the home by, and its status display name.</summary>
public sealed record HomeListItem(HomeListItemView Home, string Label, string Status);

/// <summary>Handles <see cref="GetHomes"/>.</summary>
public static class GetHomesHandler
{
    /// <summary>Returns a paginated list of homes, each labelled with its parcel's address and showing its status.</summary>
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
            // A home with no availability yet is available, so "available" is every home not known to be otherwise.
            var available = HomeStatus.Available.DisplayName;
            var statusNames = request.Statuses.Select(s => s.DisplayName).ToList();
            var otherwise = await session.Query<HomeAvailabilityView>()
                .Where(v => v.Status != available)
                .Select(v => new { v.Id, v.Status })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (statusNames.Contains(available))
            {
                var excluded = otherwise.Where(v => !statusNames.Contains(v.Status)).Select(v => v.Id).ToList();
                query = query.Where(h => !excluded.Contains(h.Id));
            }
            else
            {
                var included = otherwise.Where(v => statusNames.Contains(v.Status)).Select(v => v.Id).ToList();
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
        var statuses = availabilities.ToDictionary(a => a.Id, a => a.Status);

        return new PaginatedResponse<HomeListItem>
        {
            Items = [.. page.Select(h => new HomeListItem(
                h,
                HomeLabel.Format(addresses.GetValueOrDefault(h.LandParcelId, string.Empty), h.UnitDesignation),
                statuses.GetValueOrDefault(h.Id, HomeStatus.Available.DisplayName)))],
            TotalCount = (int)page.TotalItemCount,
            PageSize = (int)page.PageSize,
            PageNumber = (int)page.PageNumber,
            TotalPages = (int)page.PageCount,
        };
    }
}
