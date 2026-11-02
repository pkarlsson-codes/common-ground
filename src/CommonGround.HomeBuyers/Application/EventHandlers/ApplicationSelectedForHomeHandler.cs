using CommonGround.HomeBuyers.Contracts.Events;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;

namespace CommonGround.HomeBuyers.Application.EventHandlers;

/// <summary>Publishes the ApplicationSelected integration event when an applicant is selected for a home.</summary>
public static class ApplicationSelectedForHomeHandler
{
    /// <summary>Maps the domain event to the integration event Wolverine publishes.</summary>
    public static ApplicationSelected Handle(ApplicationSelectedForHome @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return new ApplicationSelected(
            @event.AggregateId,
            @event.HouseholdId,
            @event.ApplicantFullName,
            @event.ApplicantEmail,
            @event.ApplicantPhone,
            @event.HomeSelectionId,
            @event.HomeId,
            @event.LandParcelId,
            @event.GroundLeaseTemplateId,
            @event.PurchasePrice,
            @event.SelectedDate,
            @event.SelectedBy);
    }
}
