using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeaseTemplates.Events;

public sealed record GroundLeaseTemplateRestored(
    string AggregateId,
    string RestoredBy) : DomainEvent(AggregateId);
