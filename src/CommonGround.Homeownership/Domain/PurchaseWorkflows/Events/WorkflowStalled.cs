using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

public sealed record WorkflowStalled(
    string AggregateId,
    string StalledReason,
    DateOnly StalledDate,
    DateOnly ResolutionTargetDate,
    string RecordedBy) : DomainEvent(AggregateId);
