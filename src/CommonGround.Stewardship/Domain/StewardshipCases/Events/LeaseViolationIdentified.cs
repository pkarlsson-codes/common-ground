using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

public sealed record LeaseViolationIdentified(
    string AggregateId,
    Guid ViolationId,
    LeaseViolationType ViolationType,
    string Description,
    DateOnly IdentifiedDate,
    string IdentifiedBy) : DomainEvent(AggregateId);
