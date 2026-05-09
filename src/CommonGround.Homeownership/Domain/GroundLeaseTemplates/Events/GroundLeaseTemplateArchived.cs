using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeaseTemplates.Events;

/// <summary>Raised when a ground lease template is archived.</summary>
public sealed record GroundLeaseTemplateArchived(
    string AggregateId,
    string ArchivedBy) : DomainEvent(AggregateId);
