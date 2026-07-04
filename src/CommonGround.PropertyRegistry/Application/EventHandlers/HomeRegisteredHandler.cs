using Wolverine;
using DomainHomeRegistered = CommonGround.PropertyRegistry.Domain.Homes.Events.HomeRegistered;
using IntegrationHomeRegistered = CommonGround.PropertyRegistry.Contracts.Events.HomeRegistered;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Publishes the HomeRegistered integration event when a home is registered.</summary>
public sealed class HomeRegisteredHandler(IMessageBus bus)
{
    /// <summary>Maps domain event to integration event and publishes it.</summary>
    public async Task Handle(DomainHomeRegistered @event, CancellationToken cancellationToken)
    {
        await bus.PublishAsync(new IntegrationHomeRegistered(
            @event.AggregateId,
            @event.LandParcelId)).ConfigureAwait(false);
    }
}
