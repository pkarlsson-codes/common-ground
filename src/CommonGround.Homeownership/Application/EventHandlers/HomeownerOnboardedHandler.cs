using CommonGround.Homeownership.Domain.GroundLeases.Events;
using Wolverine;
using GroundLeaseCreatedIntegration = CommonGround.Homeownership.Contracts.Events.GroundLeaseCreated;

namespace CommonGround.Homeownership.Application.EventHandlers;

/// <summary>Publishes an integration event when a homeowner is onboarded, so other BCs can react.</summary>
public sealed class HomeownerOnboardedHandler(IMessageBus bus)
{
    /// <summary>Handles the domain event and publishes the integration event to the bus.</summary>
    public async Task Handle(GroundLeaseCreatedFromPurchase @event, CancellationToken ct)
    {
        await bus.PublishAsync(new GroundLeaseCreatedIntegration(
            @event.AggregateId,
            @event.BuyerFullName,
            @event.BuyerEmail,
            @event.BuyerPhone,
            @event.RegisteredBy))
            .ConfigureAwait(false);
    }
}
