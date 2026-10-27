using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Marten;
using Marten.Pagination;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Returns a paginated list of ground leases with optional filters, newest first.</summary>
public sealed record GetGroundLeases(
    PageSpec Page,
    IReadOnlyList<GroundLeaseStatus>? Statuses = null,
    string? LandParcelId = null,
    string? UnitId = null,
    string? Search = null);

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
            query = query.Where(v => v.LandParcelId == request.LandParcelId);
        }

        if (request.UnitId is not null)
        {
            query = query.Where(v => v.HomeId == request.UnitId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search;
            query = query.Where(v =>
                (v.HomeownerFullName != null && v.HomeownerFullName.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                (v.HomeownerEmail != null && v.HomeownerEmail.Contains(s, StringComparison.OrdinalIgnoreCase)));
        }

        var page = await query
            .OrderByDescending(v => v.CreatedAt)
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
