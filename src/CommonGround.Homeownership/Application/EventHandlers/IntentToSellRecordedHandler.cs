using CommonGround.Homeownership.Contracts.Events;
using CommonGround.Homeownership.Domain.Resales.Events;

namespace CommonGround.Homeownership.Application.EventHandlers;

/// <summary>Publishes that a resale opened, so other BCs can show the home in resale.</summary>
public static class IntentToSellRecordedHandler
{
    /// <summary>Maps the domain event to the integration event Wolverine publishes.</summary>
    public static ResaleOpened Handle(IntentToSellRecorded @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return new ResaleOpened(
            @event.AggregateId,
            @event.GroundLeaseId,
            @event.HomeId,
            @event.NoticeDate,
            @event.OptionWindowEndsOn);
    }
}
