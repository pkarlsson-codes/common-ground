using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeaseTemplates.Events;

public sealed record GroundLeaseTemplateArchived(
    string AggregateId,
    string ArchivedBy) : DomainEvent(AggregateId);
