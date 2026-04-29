using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LandParcels.Events;

/// <summary>Raised when a land parcel is listed for placement.</summary>
public sealed record ParcelListedForPlacement(
    string AggregateId,
    string ChangedBy) : DomainEvent(AggregateId);
