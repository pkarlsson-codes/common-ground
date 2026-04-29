using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LandParcels.Events;

/// <summary>Raised when construction is started on a land parcel.</summary>
public sealed record ParcelConstructionStarted(
    string AggregateId,
    string ChangedBy) : DomainEvent(AggregateId);
