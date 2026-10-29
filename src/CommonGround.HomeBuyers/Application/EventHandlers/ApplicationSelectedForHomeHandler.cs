using CommonGround.HomeBuyers.Contracts.Events;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using Wolverine;

namespace CommonGround.HomeBuyers.Application.EventHandlers;

/// <summary>Publishes the ApplicationSelected integration event when an applicant is selected for a home.</summary>
public sealed class ApplicationSelectedForHomeHandler(IMessageBus bus)
{
    /// <summary>Maps domain event to integration event and publishes it.</summary>
    public async Task Handle(ApplicationSelectedForHome @event, CancellationToken cancellationToken)
    {
        await bus.PublishAsync(new ApplicationSelected(
            @event.AggregateId,
            @event.HouseholdId,
            @event.ApplicantFullName,
            @event.ApplicantEmail,
            @event.ApplicantPhone,
            @event.HomeId,
            @event.LandParcelId,
            @event.GroundLeaseTemplateId,
            @event.PurchasePrice,
            @event.SelectedDate,
            @event.SelectedBy)).ConfigureAwait(false);
    }
}
