using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

public sealed record StewardshipCaseOpened(
    string AggregateId,
    string GroundLeaseId,
    DateOnly OpenedDate,
    string OpenedBy) : DomainEvent(AggregateId);
