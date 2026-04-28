using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LandParcels.Events;

/// <summary>Raised when a parcel is marked ready for occupancy.</summary>
public sealed record ParcelMarkedReadyForOccupancy(
    string AggregateId,
    string ChangedBy) : DomainEvent(AggregateId);
