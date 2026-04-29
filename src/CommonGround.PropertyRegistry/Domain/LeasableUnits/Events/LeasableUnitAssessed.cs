using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LeasableUnits.Events;

/// <summary>Raised when a leasable unit's assessed value is updated.</summary>
public sealed record LeasableUnitAssessed(
    string AggregateId,
    decimal AssessedValue,
    DateOnly AssessmentDate,
    string? ConditionNotes,
    string AssessedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
