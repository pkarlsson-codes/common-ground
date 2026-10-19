using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Marten;
using Marten.Pagination;

namespace CommonGround.HomeBuyers.Application.Queries;

/// <summary>Returns a paginated list of households, optionally filtered by any member's name or email.</summary>
public sealed record GetHouseholds(
    PageSpec Page,
    string? Search = null);

/// <summary>Handles <see cref="GetHouseholds"/>.</summary>
public static class GetHouseholdsHandler
{
    /// <summary>Queries households with optional filters and returns a paginated result, newest first.</summary>
    public static async Task<PaginatedResponse<HouseholdListItemView>> Handle(
        GetHouseholds request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<HouseholdListItemView> query = session.Query<HouseholdListItemView>();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search;
            query = query.Where(v => v.Members.Any(m =>
                m.Name.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                m.Email.Contains(s, StringComparison.OrdinalIgnoreCase)));
        }

        var page = await query
            .OrderByDescending(v => v.RegisteredAt)
            .ToPagedListAsync(request.Page.Number, request.Page.Size, cancellationToken)
            .ConfigureAwait(false);

        return new PaginatedResponse<HouseholdListItemView>
        {
            Items = [.. page],
            TotalCount = (int)page.TotalItemCount,
            PageSize = (int)page.PageSize,
            PageNumber = (int)page.PageNumber,
            TotalPages = (int)page.PageCount,
        };
    }
}
