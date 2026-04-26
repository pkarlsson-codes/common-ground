using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Documents.Domain.Documents.Events;

/// <summary>Raised when a Document is attached to an entity in another BC.</summary>
public sealed record DocumentAttached(
    string AggregateId,
    string EntityType,
    Guid EntityId,
    string? Role,
    string AttachedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
