using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

/// <summary>Raised when financing is committed by a lender.</summary>
public sealed record FinancingCommitted(
    string AggregateId,
    string LenderName,
    decimal CommittedAmount,
    DateOnly PreapprovalLetterDate,
    string RecordedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
