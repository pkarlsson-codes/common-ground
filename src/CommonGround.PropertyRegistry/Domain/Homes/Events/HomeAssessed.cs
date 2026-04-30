using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.Homes.Events;

/// <summary>Raised when a leasable unit's assessed value is updated.</summary>
public sealed record HomeAssessed(
    string AggregateId,
    decimal AssessedValue,
    DateOnly AssessmentDate,
    string? AppraiserName,
    string? ConditionNotes,
    string RecordedById)
: DomainEvent(AggregateId);
