using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

/// <summary>
/// Raised when a recorded eligibility verification is corrected. Carries the full set of values that now stand, laid
/// out as <see cref="BuyerEligibilityVerified"/> lays them out, so the limit and band are those the corrected income was
/// checked against. <paramref name="Reason"/> is why staff corrected it.
/// </summary>
public sealed record BuyerEligibilityCorrected(
    string AggregateId,
    DateOnly VerifiedDate,
    string? EligibilityNotes,
    Money VerifiedAnnualIncome,
    int VerifiedHouseholdSize,
    Money? IncomeLimit,
    int? IncomeBandOverPercentOfAmi,
    int? IncomeBandUpToPercentOfAmi,
    IReadOnlyList<string> IncomeDocumentIds,
    string Reason,
    string CorrectedBy) : DomainEvent(AggregateId);
