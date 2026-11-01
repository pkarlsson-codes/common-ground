using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.SharedKernel.Infrastructure.API;
using CommonGround.SharedKernel.Infrastructure.Querying;
using Marten;
using Marten.Pagination;

namespace CommonGround.HomeBuyers.Application.Queries;

/// <summary>Returns home selections across every home, newest first, optionally filtered by status.</summary>
public sealed record GetHomeSelections(
    PageSpec Page,
    string? Status = null);

/// <summary>A home selection with the home's label and the chosen household's primary contact, once chosen.</summary>
public sealed record HomeSelectionListItem(
    HomeSelectionView Selection,
    string? HomeLabel,
    HouseholdContact? ChosenContact);

/// <summary>Handles <see cref="GetHomeSelections"/>.</summary>
public static class GetHomeSelectionsHandler
{
    /// <summary>Queries the selections and joins each with its home's label and chosen household.</summary>
    public static async Task<PaginatedResponse<HomeSelectionListItem>> Handle(
        GetHomeSelections request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<HomeSelectionView> query = session.Query<HomeSelectionView>();

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(v => v.Status == request.Status);
        }

        var page = await query
            .OrderByDescending(v => v.StartedAt)
            .ToPagedListAsync(request.Page.Number, request.Page.Size, cancellationToken)
            .ConfigureAwait(false);

        var homes = (await session
            .LoadManyAsync<HomeFactsView>(cancellationToken, page.Select(s => s.HomeId).Distinct())
            .ConfigureAwait(false))
            .ToDictionary(h => h.Id);

        var chosenHouseholdIds = page
            .Select(s => s.Candidates.FirstOrDefault(c => c.ApplicationId == s.SelectedApplicationId)?.HouseholdId)
            .OfType<string>()
            .Distinct();
        var households = (await session
            .LoadManyAsync<HouseholdListItemView>(cancellationToken, chosenHouseholdIds)
            .ConfigureAwait(false))
            .ToDictionary(h => h.Id);

        return new PaginatedResponse<HomeSelectionListItem>
        {
            Items = [.. page.Select(s =>
            {
                var chosenHouseholdId = s.Candidates.FirstOrDefault(c => c.ApplicationId == s.SelectedApplicationId)?.HouseholdId;
                var household = chosenHouseholdId is null ? null : households.GetValueOrDefault(chosenHouseholdId);
                return new HomeSelectionListItem(
                    s,
                    homes.GetValueOrDefault(s.HomeId)?.Label,
                    household is null ? null : HouseholdContact.From(household));
            })],
            TotalCount = (int)page.TotalItemCount,
            PageSize = (int)page.PageSize,
            PageNumber = (int)page.PageNumber,
            TotalPages = (int)page.PageCount,
        };
    }
}
