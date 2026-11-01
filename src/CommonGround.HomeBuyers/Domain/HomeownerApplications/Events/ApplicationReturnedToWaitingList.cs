using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;

/// <summary>
/// The purchase of the home the applicant was selected for fell through, so the application is approved
/// and waiting again, keeping its original approval date and so its place on the waiting list.
/// </summary>
public sealed record ApplicationReturnedToWaitingList(
    string AggregateId,
    string HouseholdId,
    string HomeId,
    string Reason,
    DateOnly ReturnedDate) : DomainEvent(AggregateId);
