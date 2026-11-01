using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.Homes.Events;

/// <summary>Raised when a household is selected for a home, putting it under contract.</summary>
public sealed record HomeUnderContract(
    string AggregateId,
    DateOnly ContractDate) : DomainEvent(AggregateId);
