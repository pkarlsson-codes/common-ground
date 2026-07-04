using CommonGround.ApplicantSelection.Application.ReadModels;
using CommonGround.PropertyRegistry.Contracts.Events;
using Marten;

namespace CommonGround.ApplicantSelection.Application.EventHandlers;

/// <summary>Seeds <see cref="HomeLandParcelView"/> from PropertyRegistry's HomeRegistered integration event.</summary>
public static class HomeRegisteredHandler
{
    /// <summary>Records the home's land parcel for later use in applicant selection.</summary>
    public static void Handle(HomeRegistered @event, IDocumentSession session)
    {
        ArgumentNullException.ThrowIfNull(@event);

        session.Store(new HomeLandParcelView
        {
            Id = @event.HomeId,
            LandParcelId = @event.LandParcelId,
        });
    }
}
