using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;

/// <summary>Raised when a servicer delinquency is recorded against the case.</summary>
/// <param name="AggregateId">The foreclosure prevention case identifier.</param>
/// <param name="ServicerName">The name of the mortgage servicer.</param>
/// <param name="DelinquentMonths">The number of months delinquent.</param>
/// <param name="RecordedDate">The date the delinquency was recorded.</param>
/// <param name="RecordedBy">The staff member who recorded the delinquency.</param>
/// <param name="OccurredAt">When the event occurred.</param>
public sealed record ServicerDelinquencyRecorded(
    string AggregateId,
    string ServicerName,
    int DelinquentMonths,
    DateOnly RecordedDate,
    string RecordedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
