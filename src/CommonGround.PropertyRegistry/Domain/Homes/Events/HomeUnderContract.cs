using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.Homes.Events;

/// <summary>Raised when a home marked under contract.</summary>
public sealed record HomeUnderContract(
    string AggregateId) : DomainEvent(AggregateId);