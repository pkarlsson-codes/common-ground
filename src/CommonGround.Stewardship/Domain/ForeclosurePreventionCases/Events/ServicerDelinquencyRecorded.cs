using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;

public sealed record ServicerDelinquencyRecorded(
    string AggregateId,
    string ServicerName,
    int DelinquentMonths,
    DateOnly RecordedDate,
    string RecordedBy) : DomainEvent(AggregateId);
