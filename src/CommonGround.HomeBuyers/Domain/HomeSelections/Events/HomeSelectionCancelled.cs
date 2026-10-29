using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeSelections.Events;

/// <summary>Staff cancelled the selection without choosing a household.</summary>
public sealed record HomeSelectionCancelled(
    string AggregateId,
    string Reason,
    string CancelledBy) : DomainEvent(AggregateId);
