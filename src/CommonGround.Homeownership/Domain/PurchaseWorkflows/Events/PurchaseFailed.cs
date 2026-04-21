using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

/// <summary>Raised when the purchase deal is terminated.</summary>
public sealed record PurchaseFailed(
    string AggregateId,
    string FailureReason,
    DateOnly FailedDate,
    string RecordedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
