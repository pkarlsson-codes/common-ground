using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LeasableUnits.Events;

/// <summary>Raised when a resale is abandoned in favour of redevelopment for a dwelling unit.</summary>
public sealed record ResaleAbandonedForRedevelopment(
    string AggregateId,
    string ChangedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
