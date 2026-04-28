using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LandParcels.Events;

/// <summary>Raised when a parcel becomes occupied.</summary>
public sealed record ParcelOccupied(
    string AggregateId,
    string ChangedBy) : DomainEvent(AggregateId);
