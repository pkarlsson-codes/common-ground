using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;

public sealed record ForbearanceRequested(
    string AggregateId,
    Guid RequestId,
    string LenderName,
    DateOnly RequestedDate,
    string RequestedBy) : DomainEvent(AggregateId);
