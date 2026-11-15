using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

/// <summary>Raised when a recorded financing commitment is corrected. Carries the full set of values that now stand and the reason.</summary>
public sealed record FinancingCorrected(
    string AggregateId,
    string LenderName,
    decimal CommittedAmount,
    DateOnly PreapprovalLetterDate,
    string Reason,
    string CorrectedBy) : DomainEvent(AggregateId);
