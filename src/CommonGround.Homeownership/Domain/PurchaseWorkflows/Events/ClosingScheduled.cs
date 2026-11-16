using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

public sealed record ClosingScheduled(
    string AggregateId,
    DateOnly ScheduledClosingDate,
    string RecordedBy) : DomainEvent(AggregateId);
