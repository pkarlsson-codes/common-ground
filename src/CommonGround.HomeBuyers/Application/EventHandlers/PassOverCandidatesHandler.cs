using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.HomeBuyers.Application.EventHandlers;

/// <summary>
/// When a home selection chooses a household, passes over every other candidate, one message each,
/// so completing the selection writes only the selection and the chosen application. Selections that
/// share candidates can then complete at the same time without conflicting.
/// </summary>
public static class PassOverCandidatesHandler
{
    /// <summary>Loads the selection the household was chosen in.</summary>
    public static async Task<HomeSelectionView> LoadAsync(
        ApplicationSelectedForHome @event,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(session);

        return await session
            .LoadAsync<HomeSelectionView>(@event.HomeSelectionId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(typeof(HomeSelection), @event.HomeSelectionId);
    }

    /// <summary>
    /// Asks for every candidate except the chosen one and those over the home's income limit to be passed
    /// over. An over-limit household didn't lose to another household; it could never have been chosen.
    /// </summary>
    public static IEnumerable<PassOverApplication> Handle(ApplicationSelectedForHome @event, HomeSelectionView selection)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(selection);

        var selectionId = HomeSelectionId.Create(@event.HomeSelectionId);
        return
        [
            .. selection.Candidates
                .Where(c => c.ApplicationId != @event.AggregateId && !c.IsOverIncomeLimit)
                .Select(c => new PassOverApplication(
                    HomeownerApplicationId.Create(c.ApplicationId), selectionId, @event.HomeId, @event.SelectedDate)),
        ];
    }
}
