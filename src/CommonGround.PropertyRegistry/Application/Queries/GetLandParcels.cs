using CommonGround.PropertyRegistry.Application.ReadModels;
using System.Globalization;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Marten;
using Marten.Pagination;

namespace CommonGround.PropertyRegistry.Application.Queries;

/// <summary>Returns a paginated list of land parcels with optional filters.</summary>
/// <param name="Page">Pagination spec (page number + size).</param>
/// <param name="Search">Optional text search across legal parcel number and address.</param>
/// <param name="IncludeDecommissioned">Whether to include decommissioned parcels.</param>
public sealed record GetLandParcels(
    PageSpec Page,
    string? Search = null,
    bool IncludeDecommissioned = false);

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

        if (!request.IncludeDecommissioned)
        {
            query = query.Where(p => !p.IsDecommissioned);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower(CultureInfo.InvariantCulture);
            query = query.Where(p =>
                p.Address.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                p.LegalParcelNumber.Contains(s, StringComparison.OrdinalIgnoreCase));
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
