using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LandParcels.Events;

/// <summary>Raised when a parcel enters a transitioning state.</summary>
public sealed record ParcelEnteredTransitioning(
    string AggregateId,
    string ChangedBy) : DomainEvent(AggregateId);
