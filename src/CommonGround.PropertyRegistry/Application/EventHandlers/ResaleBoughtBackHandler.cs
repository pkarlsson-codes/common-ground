using CommonGround.Homeownership.Contracts.Events;
using CommonGround.PropertyRegistry.Application.ReadModels;
using Marten;
using Wolverine.Marten;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Shows a home vacant again when the trust buys it back, offered to the waiting list or held.</summary>
public static class ResaleBoughtBackHandler
{
    /// <summary>Loads the home's availability.</summary>
    public static Task<HomeAvailabilityView?> LoadAsync(ResaleBoughtBack @event, IDocumentSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return HomeAvailabilityUpdate.LoadAsync(session, @event.HomeId, cancellationToken);
    }

    /// <summary>Records the buyback on the home's availability.</summary>
    public static IMartenOp Handle(ResaleBoughtBack @event, HomeAvailabilityView? availability)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return HomeAvailabilityUpdate.Record(
            @event.HomeId,
            availability,
            view => view.RecordResaleBoughtBack(@event.ResaleId, @event.ClosingDate, @event.OfferedForSale, @event.HoldReason));
    }
}
