using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LandParcels.Events;

/// <summary>Raised when parcel construction starts.</summary>
public sealed record ParcelConstructionStarted(
    string AggregateId,
    string ChangedBy) : DomainEvent(AggregateId);
