using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LeasableUnits.Events;

/// <summary>Raised when a ground lease is executed on a dwelling unit.</summary>
public sealed record GroundLeaseExecuted(
    string AggregateId,
    string ChangedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
