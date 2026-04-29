using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LandParcels.Events;

/// <summary>Raised when construction is completed on a land parcel.</summary>
public sealed record ConstructionCompleted(
    string AggregateId,
    string ChangedBy) : DomainEvent(AggregateId);
