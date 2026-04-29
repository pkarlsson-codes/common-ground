using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LeasableUnits.Events;

/// <summary>Raised when a pending occupancy is cancelled for a dwelling unit.</summary>
public sealed record OccupancyCancelled(
    string AggregateId,
    string ChangedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
