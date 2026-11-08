using CommonGround.Homeownership.Contracts.Events;
using CommonGround.Homeownership.Domain.Resales.Events;

namespace CommonGround.Homeownership.Application.EventHandlers;

/// <summary>Publishes that the trust bought a home back, so other BCs can show it vacant again.</summary>
public static class BuybackClosedHandler
{
    /// <summary>Maps the domain event to the integration event Wolverine publishes.</summary>
    public static ResaleBoughtBack Handle(BuybackClosed @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return new ResaleBoughtBack(
            @event.AggregateId,
            @event.GroundLeaseId,
            @event.HomeId,
            @event.ClosingDate,
            @event.OfferedForSale,
            @event.HoldReason);
    }
}
