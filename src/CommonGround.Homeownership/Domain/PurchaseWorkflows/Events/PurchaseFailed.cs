using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

public sealed record PurchaseFailed(
    string AggregateId,
    string FailureReason,
    DateOnly FailedDate,
    string RecordedBy) : DomainEvent(AggregateId);
