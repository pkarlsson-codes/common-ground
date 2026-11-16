using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;

public sealed record CounselingOutcomeRecorded(
    string AggregateId,
    Guid ReferralId,
    string Outcome,
    DateOnly OutcomeDate,
    string RecordedBy) : DomainEvent(AggregateId);
