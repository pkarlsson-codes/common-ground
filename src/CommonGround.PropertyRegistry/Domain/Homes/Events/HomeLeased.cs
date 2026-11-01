using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.Homes.Events;

/// <summary>Raised when the home's purchase closes and it is leased to its homeowner.</summary>
public sealed record HomeLeased(
    string AggregateId,
    DateOnly LeasedDate) : DomainEvent(AggregateId);
