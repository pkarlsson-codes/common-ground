using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.Homes;
using Marten;
using Wolverine;
using DomainHomeRegistered = CommonGround.PropertyRegistry.Domain.Homes.Events.HomeRegistered;
using IntegrationHomeRegistered = CommonGround.PropertyRegistry.Contracts.Events.HomeRegistered;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Publishes the HomeRegistered integration event when a home is registered.</summary>
public sealed class HomeRegisteredHandler(IMessageBus bus)
{
    /// <summary>Maps domain event to integration event and publishes it.</summary>
    public async Task Handle(DomainHomeRegistered @event, IQuerySession session, CancellationToken cancellationToken)
    {
        var parcel = await session.LoadAsync<LandParcelView>(@event.LandParcelId, cancellationToken).ConfigureAwait(false);

        await bus.PublishAsync(new IntegrationHomeRegistered(
            @event.AggregateId,
            @event.LandParcelId,
            HomeLabel.Format(parcel?.Address ?? string.Empty, @event.UnitDesignation),
            @event.Bedrooms,
            @event.IsAccessible)).ConfigureAwait(false);
    }
}
