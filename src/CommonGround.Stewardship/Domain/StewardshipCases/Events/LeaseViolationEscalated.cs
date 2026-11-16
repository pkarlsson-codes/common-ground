using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

public sealed record LeaseViolationEscalated(
    string AggregateId,
    Guid ViolationId,
    string Reason,
    DateOnly EscalatedDate,
    string EscalatedBy) : DomainEvent(AggregateId);
