using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.Events;

/// <summary>Raised when both parties sign the ground lease.</summary>
public sealed record LeaseExecuted(
    string AggregateId,
    DateOnly SignedDate,
    string RecordedBy) : DomainEvent(AggregateId);
