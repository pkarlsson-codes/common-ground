using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;

/// <summary>The purchase of the home the applicant was selected for closed, so the household owns its home.</summary>
public sealed record ApplicationPurchaseClosed(
    string AggregateId,
    string HouseholdId,
    string HomeId,
    DateOnly ClosingDate) : DomainEvent(AggregateId);
