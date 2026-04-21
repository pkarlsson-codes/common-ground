using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

/// <summary>Raised when the purchase workflow is stalled/blocked.</summary>
public sealed record WorkflowStalled(
    string AggregateId,
    string StalledReason,
    DateOnly StalledDate,
    DateOnly ResolutionTargetDate,
    string RecordedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
