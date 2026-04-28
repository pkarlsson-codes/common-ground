using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LandParcels.Events;

/// <summary>Raised when a parcel returns to available.</summary>
public sealed record ParcelReturnedToAvailable(
    string AggregateId,
    string ChangedBy) : DomainEvent(AggregateId);
