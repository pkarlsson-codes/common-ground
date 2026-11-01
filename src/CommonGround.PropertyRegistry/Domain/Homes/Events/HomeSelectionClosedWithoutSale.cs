using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.Homes.Events;

/// <summary>Raised when the home's selection is cancelled without choosing anyone, making the home available again.</summary>
public sealed record HomeSelectionClosedWithoutSale(
    string AggregateId,
    string HomeSelectionId) : DomainEvent(AggregateId);
