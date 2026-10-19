using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;

/// <summary>Application submitted event.</summary>
public sealed record ApplicationSubmitted(
    string AggregateId,
    string HouseholdId,
    int HouseholdSize,
    decimal AnnualIncome,
    bool AccessibilityRequired,
    string SubmittedBy) : DomainEvent(AggregateId);
