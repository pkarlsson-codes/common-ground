using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.PropertyRegistry.Contracts.Events;
using Marten;

namespace CommonGround.HomeBuyers.Application.EventHandlers;

/// <summary>Seeds <see cref="HomeFactsView"/> from PropertyRegistry's HomeRegistered integration event.</summary>
public static class HomeRegisteredHandler
{
    /// <summary>Records the home's facts for later use in applicant selection.</summary>
    public static void Handle(HomeRegistered @event, IDocumentSession session)
    {
        ArgumentNullException.ThrowIfNull(@event);

        session.Store(new HomeFactsView
        {
            Id = @event.HomeId,
            LandParcelId = @event.LandParcelId,
            Bedrooms = @event.Bedrooms,
            IsAccessible = @event.IsAccessible,
        });
    }
}
