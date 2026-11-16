using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LandParcels.Events;

public sealed record ParcelListedForPlacement(
    string AggregateId,
    string ChangedBy) : DomainEvent(AggregateId);
