using CommonGround.Homeownership.Domain.GroundLeases.Events;
using Wolverine;
using GroundLeaseTerminatedIntegration = CommonGround.Homeownership.Contracts.Events.GroundLeaseTerminated;

namespace CommonGround.Homeownership.Application.EventHandlers;

/// <summary>Publishes an integration event when a ground lease is terminated, so other BCs can react.</summary>
public sealed class GroundLeaseTerminatedHandler(IMessageBus bus)
{
    /// <summary>Handles the domain event and publishes the integration event to the bus.</summary>
    public async Task Handle(GroundLeaseTerminated @event, CancellationToken ct)
    {
        await bus.PublishAsync(new GroundLeaseTerminatedIntegration(
            @event.AggregateId,
            @event.Reason,
            @event.TerminatedDate,
            @event.TerminatedBy)).ConfigureAwait(false);
    }
}
