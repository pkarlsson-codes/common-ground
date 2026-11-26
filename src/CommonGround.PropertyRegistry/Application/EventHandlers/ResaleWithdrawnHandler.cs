using CommonGround.Homeownership.Contracts.Events;
using CommonGround.PropertyRegistry.Application.ReadModels;
using Marten;
using Wolverine;
using Wolverine.Marten;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Shows a home leased again when its homeowner withdraws their notice to sell.</summary>
public static class ResaleWithdrawnHandler
{
    /// <summary>Loads the home's availability.</summary>
    public static Task<HomeAvailabilityView?> LoadAsync(ResaleWithdrawn @event, IDocumentSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return HomeAvailabilityUpdate.LoadAsync(session, @event.HomeId, cancellationToken);
    }

    /// <summary>Records the withdrawal on the home's availability.</summary>
    public static (IMartenOp, OutgoingMessages) Handle(ResaleWithdrawn @event, HomeAvailabilityView? availability)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return HomeAvailabilityUpdate.Record(@event.HomeId, availability, view => view.RecordResaleWithdrawn(@event.ResaleId));
    }
}
