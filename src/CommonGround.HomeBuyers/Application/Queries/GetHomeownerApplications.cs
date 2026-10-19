using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Marten;
using Marten.Pagination;

namespace CommonGround.HomeBuyers.Application.Queries;

/// <summary>Returns a paginated list of homeowner applications with optional filters.</summary>
public sealed record GetHomeownerApplications(
    PageSpec Page,
    string? Phase = null);

/// <summary>Handles <see cref="GetHomeownerApplications"/>.</summary>
public static class GetHomeownerApplicationsHandler
{
    /// <summary>Queries homeowner applications with optional filters and returns a paginated result.</summary>
    public static async Task<PaginatedResponse<HomeownerApplicationSummary>> Handle(
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

        var page = await query
            .OrderByDescending(v => v.SubmittedAt)
            .ToPagedListAsync(request.Page.Number, request.Page.Size, cancellationToken)
            .ConfigureAwait(false);

        var households = await session
            .LoadManyAsync<HouseholdListItemView>(cancellationToken, page.Select(a => a.HouseholdId).Distinct())
            .ConfigureAwait(false);
        var householdsById = households.ToDictionary(h => h.Id);

        return new PaginatedResponse<HomeownerApplicationSummary>
        {
            Items = [.. page.Select(a => new HomeownerApplicationSummary(
                a,
                householdsById.TryGetValue(a.HouseholdId, out var household) ? HouseholdContact.From(household) : null))],
            TotalCount = (int)page.TotalItemCount,
            PageSize = (int)page.PageSize,
            PageNumber = (int)page.PageNumber,
            TotalPages = (int)page.PageCount,
        };
    }
}