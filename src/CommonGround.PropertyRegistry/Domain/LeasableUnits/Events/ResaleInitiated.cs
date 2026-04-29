using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LeasableUnits.Events;

/// <summary>Raised when a dwelling unit is listed for resale.</summary>
public sealed record ResaleInitiated(
    string AggregateId,
    string ChangedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
