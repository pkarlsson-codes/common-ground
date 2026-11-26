using CommonGround.Homeownership.Contracts.Events;
using CommonGround.PropertyRegistry.Application.ReadModels;
using Marten;
using Wolverine;
using Wolverine.Marten;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Shows a home available again when its purchase fails or is withdrawn.</summary>
public static class PurchaseFellThroughHandler
{
    /// <summary>Loads the home's availability.</summary>
    public static Task<HomeAvailabilityView?> LoadAsync(PurchaseFellThrough @event, IDocumentSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return HomeAvailabilityUpdate.LoadAsync(session, @event.HomeId, cancellationToken);
    }

    /// <summary>Records the failed purchase on the home's availability.</summary>
    public static (IMartenOp, OutgoingMessages) Handle(PurchaseFellThrough @event, HomeAvailabilityView? availability)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return HomeAvailabilityUpdate.Record(@event.HomeId, availability, view => view.RecordPurchaseFellThrough(@event.ApplicationId, @event.FellThroughDate));
    }
}
