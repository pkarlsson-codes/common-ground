using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;

public sealed record ForbearanceOutcomeRecorded(
    string AggregateId,
    Guid RequestId,
    ForbearanceOutcome Outcome,
    DateOnly OutcomeDate,
    string? Notes,
    string RecordedBy) : DomainEvent(AggregateId);
