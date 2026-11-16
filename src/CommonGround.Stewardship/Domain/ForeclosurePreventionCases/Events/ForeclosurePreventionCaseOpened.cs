using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;

public sealed record ForeclosurePreventionCaseOpened(
    string AggregateId,
    string GroundLeaseId,
    ForeclosurePreventionTrigger Trigger,
    DateOnly OpenedDate,
    string OpenedBy) : DomainEvent(AggregateId);
