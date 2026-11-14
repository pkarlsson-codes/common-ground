using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

/// <summary>
/// Raised when buyer eligibility requirements are verified. <paramref name="IncomeLimit"/> is the limit for
/// <paramref name="VerifiedHouseholdSize"/> the income was checked against, null when the purchase has no limit.
/// <paramref name="IncomeBandOverPercentOfAmi"/> and <paramref name="IncomeBandUpToPercentOfAmi"/> are the income band
/// in the purchase's schedule (see <see cref="IncomeBand"/>); both are null when the purchase carries no schedule.
/// <paramref name="IncomeDocumentIds"/> are the Documents the income was verified from, at least one.
/// </summary>
public sealed record BuyerEligibilityVerified(
    string AggregateId,
    DateOnly VerifiedDate,
    string? EligibilityNotes,
    string RecordedBy,
    Money VerifiedAnnualIncome,
    int VerifiedHouseholdSize,
    Money? IncomeLimit,
    int? IncomeBandOverPercentOfAmi,
    int? IncomeBandUpToPercentOfAmi,
    IReadOnlyList<string> IncomeDocumentIds) : DomainEvent(AggregateId);
