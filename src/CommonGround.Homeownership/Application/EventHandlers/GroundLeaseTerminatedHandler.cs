using CommonGround.Homeownership.Domain.GroundLeases.Events;
using GroundLeaseTerminatedIntegration = CommonGround.Homeownership.Contracts.Events.GroundLeaseTerminated;

namespace CommonGround.Homeownership.Application.EventHandlers;

/// <summary>Publishes an integration event when a ground lease is terminated, so other BCs can react.</summary>
public static class GroundLeaseTerminatedHandler
{
    /// <summary>Maps the domain event to the integration event Wolverine publishes.</summary>
    public static GroundLeaseTerminatedIntegration Handle(GroundLeaseTerminated @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return new GroundLeaseTerminatedIntegration(
            @event.AggregateId,
            @event.Reason,
            @event.TerminatedDate,
            @event.TerminatedBy);
    }
}
