using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Documents.Domain.Documents.Events;

/// <summary>Raised when a Document is detached from an entity.</summary>
public sealed record DocumentDetached(
    string AggregateId,
    string EntityType,
    Guid EntityId,
    string? Role,
    string DetachedBy) : DomainEvent(AggregateId);
