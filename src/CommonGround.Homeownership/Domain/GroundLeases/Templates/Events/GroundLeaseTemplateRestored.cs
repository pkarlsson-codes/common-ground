using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Templates.Events;

/// <summary>Raised when a ground lease template is restored.</summary>
public sealed record GroundLeaseTemplateRestored(
    string AggregateId,
    string RestoredBy) : DomainEvent(AggregateId);
