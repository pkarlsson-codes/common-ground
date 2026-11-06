using CommonGround.Homeownership.Domain.Resales.Events;
using ResaleWithdrawnIntegration = CommonGround.Homeownership.Contracts.Events.ResaleWithdrawn;

namespace CommonGround.Homeownership.Application.EventHandlers;

/// <summary>Publishes that a resale was withdrawn, so other BCs can show the home leased again.</summary>
public static class ResaleWithdrawnHandler
{
    /// <summary>Maps the domain event to the integration event Wolverine publishes.</summary>
    public static ResaleWithdrawnIntegration Handle(ResaleWithdrawn @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return new ResaleWithdrawnIntegration(
            @event.AggregateId,
            @event.GroundLeaseId,
            @event.HomeId,
            @event.WithdrawnDate);
    }
}
