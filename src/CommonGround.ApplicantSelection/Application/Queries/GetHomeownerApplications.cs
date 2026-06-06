using CommonGround.ApplicantSelection.Application.ReadModels;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Marten;
using Marten.Pagination;

namespace CommonGround.ApplicantSelection.Application.Queries;

/// <summary>Returns a paginated list of homeowner applications with optional filters.</summary>
public sealed record GetHomeownerApplications(
    PageSpec Page,
    string? Phase = null,
    string? Search = null);

/// <summary>Handles <see cref="GetHomeownerApplications"/>.</summary>
public static class GetHomeownerApplicationsHandler
{
    /// <summary>Queries homeowner applications with optional filters and returns a paginated result.</summary>
    public static async Task<PaginatedResponse<HomeownerApplicationListItemView>> Handle(
        GetHomeownerApplications request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<HomeownerApplicationListItemView> query = session.Query<HomeownerApplicationListItemView>();

        if (!string.IsNullOrWhiteSpace(request.Phase))
        {
            query = query.Where(v => v.Phase == request.Phase);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLowerInvariant();
#pragma warning disable CA1862 // ToLowerInvariant().Contains() is intentional — Marten translates this to SQL; StringComparison overload is not supported by the query provider
            query = query.Where(v =>
                v.ApplicantFullName.ToLowerInvariant().Contains(search) ||
                v.ApplicantEmail.ToLowerInvariant().Contains(search));
#pragma warning restore CA1862
        }

        var page = await query
            .ToPagedListAsync(request.Page.Number, request.Page.Size, cancellationToken)
            .ConfigureAwait(false);

        return new PaginatedResponse<HomeownerApplicationListItemView>
        {
            Items = [.. page],
            TotalCount = (int)page.TotalItemCount,
            PageSize = (int)page.PageSize,
            PageNumber = (int)page.PageNumber,
            TotalPages = (int)page.PageCount,
        };
    }
}