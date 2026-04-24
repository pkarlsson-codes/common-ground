using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.PropertyRegistry.Domain.Buildings;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Marten;
using Marten.Pagination;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Returns a paginated list of ground leases with optional filters.</summary>
public sealed record GetGroundLeases(
    PageSpec Page,
    IReadOnlyList<GroundLeaseStatus>? Statuses = null,
    LandParcelId? LandParcelId = null,
    BuildingId? BuildingId = null);

/// <summary>Handles <see cref="GetGroundLeases"/>.</summary>
public static class GetGroundLeasesHandler
{
    /// <summary>Queries ground leases with optional filters and returns a paginated result.</summary>
    public static async Task<PaginatedResponse<GroundLeaseListItemView>> Handle(
        GetGroundLeases request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<GroundLeaseListItemView> query = session.Query<GroundLeaseListItemView>();

        if (request.Statuses is { Count: > 0 })
        {
            var statusNames = request.Statuses.Select(s => s.ToString()).ToList();
            query = query.Where(v => statusNames.Contains(v.Status));
        }

        if (request.LandParcelId is not null)
        {
            query = query.Where(v => v.LandParcelId == request.LandParcelId.Value);
        }

        if (request.BuildingId is not null)
        {
            query = query.Where(v => v.BuildingId == request.BuildingId.Value);
        }

        var page = await query
            .ToPagedListAsync(request.Page.Number, request.Page.Size, cancellationToken)
            .ConfigureAwait(false);

        return new PaginatedResponse<GroundLeaseListItemView>
        {
            Items = [.. page],
            TotalCount = (int)page.TotalItemCount,
            PageSize = (int)page.PageSize,
            PageNumber = (int)page.PageNumber,
            TotalPages = (int)page.PageCount,
        };
    }
}
