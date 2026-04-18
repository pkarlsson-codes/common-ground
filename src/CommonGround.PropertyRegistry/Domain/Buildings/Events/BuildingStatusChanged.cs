using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.Buildings.Events;

/// <summary>Raised when a building's status changes.</summary>
public sealed record BuildingStatusChanged(
    string AggregateId,
    string NewStatus,
    string ChangedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;