using CommonGround.HomeBuyers.Contracts.Events;
using CommonGround.PropertyRegistry.Application.ReadModels;
using Marten;
using Wolverine;
using Wolverine.Marten;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Shows a home under contract when a household is chosen for it.</summary>
public static class ApplicationSelectedHandler
{
    /// <summary>Loads the home's availability.</summary>
    public static Task<HomeAvailabilityView?> LoadAsync(ApplicationSelected @event, IDocumentSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return HomeAvailabilityUpdate.LoadAsync(session, @event.HomeId, cancellationToken);
    }

    /// <summary>Records the selection on the home's availability.</summary>
    public static (IMartenOp, OutgoingMessages) Handle(ApplicationSelected @event, HomeAvailabilityView? availability)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return HomeAvailabilityUpdate.Record(@event.HomeId, availability, view => view.RecordApplicationSelected(@event.HomeSelectionId, @event.ApplicationId, @event.SelectedDate, @event.OccurredAt));
    }
}
