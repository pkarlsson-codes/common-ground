using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.Homes.Events;

/// <summary>Raised when a home selection starts choosing a household for the home.</summary>
public sealed record HomeSelectionOpened(
    string AggregateId,
    string HomeSelectionId,
    DateOnly OpenedDate) : DomainEvent(AggregateId);
