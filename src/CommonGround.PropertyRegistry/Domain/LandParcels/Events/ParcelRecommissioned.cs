using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.LandParcels.Events;

public sealed record ParcelRecommissioned(
    string AggregateId,
    string ChangedBy) : DomainEvent(AggregateId);
