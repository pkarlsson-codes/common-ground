using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.HomeSelections.Events;

namespace CommonGround.HomeBuyers.Application.EventHandlers;

/// <summary>
/// When a household is chosen for a home, passes over every application the choice marked passed over, one message
/// each, so recording the choice writes only the selection, the home and the chosen application. Choices for
/// different homes that share a waiting list can then be recorded at the same time without conflicting.
/// </summary>
public static class PassOverCandidatesHandler
{
    /// <summary>Asks for each application marked passed over in a choice by priority to be passed over.</summary>
    public static IEnumerable<PassOverApplication> Handle(HouseholdSelectedByPriority @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return PassOver(@event.AggregateId, @event.HomeId, @event.Applications);
    }

    /// <summary>Asks for each application marked passed over in a lottery to be passed over.</summary>
    public static IEnumerable<PassOverApplication> Handle(HouseholdSelectedByLottery @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return PassOver(@event.AggregateId, @event.HomeId, @event.Applications);
    }

    private static List<PassOverApplication> PassOver(string selectionId, string homeId, IEnumerable<RecordedApplication> applications) =>
        [
            .. applications
                .Where(a => a.Outcome == SelectionOutcome.PassedOver)
                .Select(a => new PassOverApplication(
                    HomeownerApplicationId.Create(a.ApplicationId), HomeSelectionId.Create(selectionId), homeId)),
        ];
}
