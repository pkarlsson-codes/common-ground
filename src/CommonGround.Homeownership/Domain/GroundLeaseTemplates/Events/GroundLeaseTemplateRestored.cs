using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeaseTemplates.Events;

/// <summary>Raised when a ground lease template is restored.</summary>
public sealed record GroundLeaseTemplateRestored(
    string AggregateId,
    string RestoredBy) : DomainEvent(AggregateId);
