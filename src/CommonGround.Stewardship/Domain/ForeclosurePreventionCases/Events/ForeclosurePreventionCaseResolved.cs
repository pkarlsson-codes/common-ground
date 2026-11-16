using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;

public sealed record ForeclosurePreventionCaseResolved(
    string AggregateId,
    ForeclosurePreventionOutcome Outcome,
    DateOnly ResolvedDate,
    string ResolvedBy) : DomainEvent(AggregateId);
