using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.Homes.Events;

/// <summary>Raised when the home's purchase fails or is withdrawn, making the home available again.</summary>
public sealed record HomeContractReleased(
    string AggregateId,
    DateOnly ReleasedDate) : DomainEvent(AggregateId);
