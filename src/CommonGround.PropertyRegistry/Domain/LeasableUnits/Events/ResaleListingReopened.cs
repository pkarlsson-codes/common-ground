using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LeasableUnits.Events;

/// <summary>Raised when a resale listing is reopened for a dwelling unit.</summary>
public sealed record ResaleListingReopened(
    string AggregateId,
    string ChangedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
