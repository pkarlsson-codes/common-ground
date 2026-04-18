using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.Buildings.Events;

/// <summary>Raised when a building's assessed value is updated.</summary>
public sealed record BuildingAssessed(
    string AggregateId,
    decimal AssessedValue,
    DateOnly AssessmentDate,
    string? ConditionNotes,
    string AssessedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;