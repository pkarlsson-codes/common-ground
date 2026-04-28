using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LandParcels.Events;

/// <summary>Raised when a parcel is decommissioned.</summary>
public sealed record ParcelDecommissioned(
    string AggregateId,
    string ChangedBy) : DomainEvent(AggregateId);
