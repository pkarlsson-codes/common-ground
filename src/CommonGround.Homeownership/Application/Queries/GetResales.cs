using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.Resales;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;
using Marten.Pagination;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Returns a paginated list of resales, most recent notice first.</summary>
public sealed record GetResales(
    PageSpec Page,
    ResaleStatus? Status = null,
    GroundLeaseId? GroundLeaseId = null);

/// <summary>A resale and the label of the home being sold.</summary>
public sealed record ResaleListItem(ResaleView Resale, string? HomeLabel);

/// <summary>Handles <see cref="GetResales"/>.</summary>
public static class GetResalesHandler
{
    /// <summary>Queries resales with optional filters and returns a paginated result.</summary>
    public static async Task<PaginatedResponse<ResaleListItem>> Handle(
        GetResales request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(session);

        IQueryable<ResaleView> query = session.Query<ResaleView>();

        if (request.Status is { } status)
        {
            var statusName = status.DisplayName;
            query = query.Where(v => v.Status == statusName);
        }

        if (request.GroundLeaseId is { } groundLeaseId)
        {
            var leaseId = groundLeaseId.Value;
            query = query.Where(v => v.GroundLeaseId == leaseId);
        }

        var page = await query
            .OrderByDescending(v => v.NoticeDate)
            .ThenByDescending(v => v.RecordedAt)
            .ToPagedListAsync(request.Page.Number, request.Page.Size, cancellationToken)
            .ConfigureAwait(false);

        var homeIds = page.Select(v => v.HomeId).Distinct().ToList();
        var labels = (await session.LoadManyAsync<HomeLabelView>(cancellationToken, homeIds).ConfigureAwait(false))
            .ToDictionary(l => l.Id, l => l.Label);

        return new PaginatedResponse<ResaleListItem>
        {
            Items = [.. page.Select(v => new ResaleListItem(v, labels.GetValueOrDefault(v.HomeId)))],
            TotalCount = (int)page.TotalItemCount,
            PageSize = (int)page.PageSize,
            PageNumber = (int)page.PageNumber,
            TotalPages = (int)page.PageCount,
        };
    }
}

/// <summary>Returns one resale and the label of the home being sold.</summary>
public sealed record GetResale(ResaleId ResaleId);

/// <summary>Handles <see cref="GetResale"/>.</summary>
public static class GetResaleHandler
{
    /// <summary>Loads the resale and its home's label.</summary>
    public static async Task<ResaleListItem> Handle(GetResale request, IQuerySession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(session);

        var view = await session.LoadAsync<ResaleView>(request.ResaleId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(typeof(ResaleView), request.ResaleId.Value);
        var label = await session.LoadAsync<HomeLabelView>(view.HomeId, cancellationToken).ConfigureAwait(false);

        return new ResaleListItem(view, label?.Label);
    }
}
