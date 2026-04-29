using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LandParcels.Events;

/// <summary>Raised when a land parcel is withdrawn for redevelopment.</summary>
public sealed record ParcelWithdrawnForRedevelopment(
    string AggregateId,
    string ChangedBy) : DomainEvent(AggregateId);
