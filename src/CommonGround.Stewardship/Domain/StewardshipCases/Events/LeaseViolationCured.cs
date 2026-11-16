using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

/// <param name="CuredDate">The date the cure was verified.</param>
public sealed record LeaseViolationCured(
    string AggregateId,
    Guid ViolationId,
    DateOnly CuredDate,
    string VerifiedBy) : DomainEvent(AggregateId);
