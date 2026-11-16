using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;

public sealed record ApplicationSubmitted(
    string AggregateId,
    string HouseholdId,
    int HouseholdSize,
    Money AnnualIncome,
    bool AccessibilityRequired,
    string SubmittedBy) : DomainEvent(AggregateId);
