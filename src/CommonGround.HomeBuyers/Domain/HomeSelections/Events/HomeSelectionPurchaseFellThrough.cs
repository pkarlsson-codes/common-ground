using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeSelections.Events;

/// <summary>The purchase the selection led to failed or was withdrawn, freeing the home for a new selection.</summary>
public sealed record HomeSelectionPurchaseFellThrough(
    string AggregateId,
    string Reason,
    DateOnly FellThroughDate) : DomainEvent(AggregateId);
