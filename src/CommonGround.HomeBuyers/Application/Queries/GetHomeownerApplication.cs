using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.HomeBuyers.Application.Queries;

/// <summary>Returns a single homeowner application by ID.</summary>
public sealed record GetHomeownerApplication(HomeownerApplicationId ApplicationId);

/// <summary>Handles <see cref="GetHomeownerApplication"/>.</summary>
public static class GetHomeownerApplicationHandler
{
    /// <summary>
    /// Loads the application view by ID with its household's primary contact and pass-over history,
    /// throwing if not found.
    /// </summary>
    public static async Task<HomeownerApplicationDetails> Handle(
        GetHomeownerApplication request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var view = await session.LoadAsync<HomeownerApplicationView>(request.ApplicationId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.ApplicationId.Value);

        var household = await session.LoadAsync<HouseholdListItemView>(view.HouseholdId, cancellationToken).ConfigureAwait(false);

        var passOvers = await session.LoadAsync<HouseholdPassOversView>(view.HouseholdId, cancellationToken).ConfigureAwait(false);
        var entries = passOvers?.PassOvers ?? [];

        var selections = await session
            .LoadManyAsync<HomeSelectionView>(cancellationToken, entries.Select(p => p.SelectionId).Distinct())
            .ConfigureAwait(false);
        var selectionsById = selections.ToDictionary(s => s.Id);

        var homes = await session
            .LoadManyAsync<HomeFactsView>(cancellationToken, entries.Select(p => p.HomeId).Distinct())
            .ConfigureAwait(false);
        var homesById = homes.ToDictionary(h => h.Id);

        HouseholdPassOver ToPassOver(HouseholdPassOverEntry p)
        {
            var selection = selectionsById.GetValueOrDefault(p.SelectionId);
            return new HouseholdPassOver(
                p,
                homesById.GetValueOrDefault(p.HomeId)?.Label,
                selection?.SelectionMethod,
                selection?.SelectionReason);
        }

        return new HomeownerApplicationDetails(
            view,
            household is null ? null : HouseholdContact.From(household),
            [.. entries.Select(ToPassOver)]);
    }
}
