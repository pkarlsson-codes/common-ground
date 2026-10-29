using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Marten;
using Marten.Pagination;

namespace CommonGround.HomeBuyers.Application.Queries;

/// <summary>
/// Returns the waiting list: every approved application, longest waiting first, optionally filtered
/// by any household member's name or email. Approval places a household on the waiting list;
/// selection, denial or withdrawal takes it off.
/// </summary>
public sealed record GetWaitingList(
    PageSpec Page,
    string? Search = null);

/// <summary>Handles <see cref="GetWaitingList"/>.</summary>
public static class GetWaitingListHandler
{
    /// <summary>
    /// Queries approved applications and joins each with its household's readiness as of today.
    /// A household needs re-verification once a prequalification it held is no longer valid.
    /// </summary>
    public static async Task<PaginatedResponse<WaitingListEntry>> Handle(
        GetWaitingList request,
        IQuerySession session,
        ITrustClock trustClock,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = session.Query<HomeownerApplicationListItemView>()
            .Where(v => v.Phase == ApplicationPhase.Approved.DisplayName);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search;
            var matchingHouseholdIds = await session.Query<HouseholdListItemView>()
                .Where(v => v.Members.Any(m =>
                    m.Name.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    m.Email.Contains(s, StringComparison.OrdinalIgnoreCase)))
                .Select(v => v.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            query = query.Where(v => matchingHouseholdIds.Contains(v.HouseholdId));
        }

        var page = await query
            .OrderBy(v => v.ApprovedDate)
            .ThenBy(v => v.SubmittedAt)
            .ToPagedListAsync(request.Page.Number, request.Page.Size, cancellationToken)
            .ConfigureAwait(false);

        var households = await session
            .LoadManyAsync<HouseholdListItemView>(cancellationToken, page.Select(a => a.HouseholdId).Distinct())
            .ConfigureAwait(false);
        var householdsById = households.ToDictionary(h => h.Id);
        var today = await trustClock.TodayAsync(session.TenantId, cancellationToken).ConfigureAwait(false);

        return new PaginatedResponse<WaitingListEntry>
        {
            Items = [.. page.Select(a =>
            {
                var household = householdsById.GetValueOrDefault(a.HouseholdId);
                return new WaitingListEntry(
                    a,
                    household,
                    household?.IsReadyForSelectionOn(today) ?? false,
                    household?.NeedsReverificationOn(today) ?? false);
            })],
            TotalCount = (int)page.TotalItemCount,
            PageSize = (int)page.PageSize,
            PageNumber = (int)page.PageNumber,
            TotalPages = (int)page.PageCount,
        };
    }
}
