using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

public sealed record LeaseExecuted(
    string AggregateId,
    DateOnly SignedDate,
    string RecordedBy) : DomainEvent(AggregateId);
