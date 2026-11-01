using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Domain.Homes;
using Marten;
using DomainHomeRegistered = CommonGround.PropertyRegistry.Domain.Homes.Events.HomeRegistered;
using IntegrationHomeRegistered = CommonGround.PropertyRegistry.Contracts.Events.HomeRegistered;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Publishes the HomeRegistered integration event when a home is registered.</summary>
public static class HomeRegisteredHandler
{
    /// <summary>Maps the domain event to the integration event Wolverine publishes, labelled with the parcel's address.</summary>
    public static async Task<IntegrationHomeRegistered> Handle(DomainHomeRegistered @event, IQuerySession session, CancellationToken cancellationToken)
    {
        var parcel = await session.LoadAsync<LandParcelView>(@event.LandParcelId, cancellationToken).ConfigureAwait(false);

        return new IntegrationHomeRegistered(
            @event.AggregateId,
            @event.LandParcelId,
            HomeLabel.Format(parcel?.Address ?? string.Empty, @event.UnitDesignation),
            @event.Bedrooms,
            @event.IsAccessible);
    }
}
