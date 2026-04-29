using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LandParcels.Events;

/// <summary>Raised when construction is suspended on a land parcel.</summary>
public sealed record ConstructionSuspended(
    string AggregateId,
    string ChangedBy) : DomainEvent(AggregateId);
