using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

/// <summary>
/// Raised when buyer eligibility requirements are verified. <paramref name="IncomeLimit"/> is the limit for
/// <paramref name="VerifiedHouseholdSize"/> the income was checked against, null when the purchase has no limit.
/// </summary>
public sealed record BuyerEligibilityVerified(
    string AggregateId,
    DateOnly VerifiedDate,
    string? EligibilityNotes,
    string RecordedBy,
    Money VerifiedAnnualIncome,
    int VerifiedHouseholdSize,
    Money? IncomeLimit) : DomainEvent(AggregateId);
