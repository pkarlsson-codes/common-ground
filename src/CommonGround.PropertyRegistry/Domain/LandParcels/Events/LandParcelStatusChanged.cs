using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LandParcels.Events;

/// <summary>Raised when a land parcel's status changes.</summary>
public sealed record LandParcelStatusChanged(
    string AggregateId,
    string NewStatus,
    string ChangedBy) : DomainEvent(AggregateId);