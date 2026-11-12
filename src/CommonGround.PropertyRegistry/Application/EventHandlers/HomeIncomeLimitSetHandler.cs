using DomainHomeIncomeLimitSet = CommonGround.PropertyRegistry.Domain.Homes.Events.HomeIncomeLimitSet;
using IntegrationHomeIncomeLimitSet = CommonGround.PropertyRegistry.Contracts.Events.HomeIncomeLimitSet;

namespace CommonGround.PropertyRegistry.Application.EventHandlers;

/// <summary>Publishes the HomeIncomeLimitSet integration event when staff set a home's income limit.</summary>
public static class HomeIncomeLimitSetHandler
{
    /// <summary>Maps the domain event to the integration event Wolverine publishes.</summary>
    public static IntegrationHomeIncomeLimitSet Handle(DomainHomeIncomeLimitSet @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return new IntegrationHomeIncomeLimitSet(@event.AggregateId, @event.PercentOfAmi);
    }
}
