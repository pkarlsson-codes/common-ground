using CommonGround.Homeownership.Contracts.Events;
using CommonGround.PropertyRegistry.Application.ReadModels;
using Marten;
using Wolverine;
using Wolverine.Marten;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Shows a home in resale when its homeowner gives notice to sell.</summary>
public static class ResaleOpenedHandler
{
    /// <summary>Loads the home's availability.</summary>
    public static Task<HomeAvailabilityView?> LoadAsync(ResaleOpened @event, IDocumentSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return HomeAvailabilityUpdate.LoadAsync(session, @event.HomeId, cancellationToken);
    }

    /// <summary>Records the resale on the home's availability.</summary>
    public static (IMartenOp, OutgoingMessages) Handle(ResaleOpened @event, HomeAvailabilityView? availability)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return HomeAvailabilityUpdate.Record(@event.HomeId, availability, view => view.RecordResaleOpened(@event.ResaleId, @event.NoticeDate));
    }
}
