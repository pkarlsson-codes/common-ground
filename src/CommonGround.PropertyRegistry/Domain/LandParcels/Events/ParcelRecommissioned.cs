using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LandParcels.Events;

/// <summary>Raised when a decommissioned parcel is recommissioned.</summary>
public sealed record ParcelRecommissioned(
    string AggregateId,
    string ChangedBy) : DomainEvent(AggregateId);
