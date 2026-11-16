using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

public sealed record FinancingCommitted(
    string AggregateId,
    string LenderName,
    decimal CommittedAmount,
    DateOnly PreapprovalLetterDate,
    string RecordedBy) : DomainEvent(AggregateId);
