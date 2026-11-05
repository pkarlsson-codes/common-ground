using CommonGround.Homeownership.Domain.GroundLeases.Events;
using GroundLeaseCreatedIntegration = CommonGround.Homeownership.Contracts.Events.GroundLeaseCreated;

namespace CommonGround.Homeownership.Application.EventHandlers;

/// <summary>
/// Publishes that a lease created directly exists, as <see cref="HomeownerOnboardedHandler"/> does for one from
/// a purchase, so other BCs treat it the same way. It publishes no purchase closing: there was none.
/// </summary>
public static class GroundLeaseCreatedHandler
{
    /// <summary>Maps the domain event to the integration event Wolverine publishes.</summary>
    public static GroundLeaseCreatedIntegration Handle(GroundLeaseCreated @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return new GroundLeaseCreatedIntegration(
            @event.AggregateId,
            @event.HomeId,
            @event.LeaseStartDate,
            @event.HomeownerFullName,
            @event.HomeownerEmail,
            @event.HomeownerPhone,
            @event.RegisteredBy);
    }
}
