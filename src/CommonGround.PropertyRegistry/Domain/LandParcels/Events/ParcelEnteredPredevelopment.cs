using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LandParcels.Events;

/// <summary>Raised when a parcel enters predevelopment.</summary>
public sealed record ParcelEnteredPredevelopment(
    string AggregateId,
    string ChangedBy) : DomainEvent(AggregateId);
