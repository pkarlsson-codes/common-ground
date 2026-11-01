using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.PropertyRegistry.Contracts.Events;
using Wolverine.Marten;

namespace CommonGround.HomeBuyers.Application.EventHandlers;

/// <summary>Seeds <see cref="HomeFactsView"/> from PropertyRegistry's HomeRegistered integration event.</summary>
public static class HomeRegisteredHandler
{
    /// <summary>Records the home's facts for later use in applicant selection.</summary>
    public static IMartenOp Handle(HomeRegistered @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return MartenOps.Store(new HomeFactsView
        {
            Id = @event.HomeId,
            LandParcelId = @event.LandParcelId,
            Label = @event.Label,
            Bedrooms = @event.Bedrooms,
            IsAccessible = @event.IsAccessible,
        });
    }
}
