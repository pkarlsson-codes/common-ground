using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

/// <summary>Raised when the closing date is scheduled.</summary>
public sealed record ClosingScheduled(
    string AggregateId,
    DateOnly ScheduledClosingDate,
    string RecordedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
