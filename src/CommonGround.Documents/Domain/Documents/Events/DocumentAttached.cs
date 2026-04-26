using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Documents.Domain.Documents.Events;

/// <summary>Raised when a Document is attached to an entity in another BC.</summary>
/// <remarks>
/// Carries denormalised current-version metadata so the per-entity projection can populate
/// list rows without a cross-stream lookup.
/// </remarks>
public sealed record DocumentAttached(
    string AggregateId,
    string EntityType,
    Guid EntityId,
    string? Role,
    string Title,
    string CurrentFileName,
    string CurrentContentType,
    long CurrentSizeBytes,
    string AttachedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
