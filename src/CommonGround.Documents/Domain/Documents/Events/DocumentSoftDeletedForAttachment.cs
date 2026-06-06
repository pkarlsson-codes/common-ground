using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Documents.Domain.Documents.Events;

/// <summary>
/// Fanout event raised once per active attachment when a Document is soft-deleted.
/// Carries the (EntityType, EntityId) so the per-entity projection can route via Identity&lt;&gt;
/// and flag the row as deleted without a cross-stream lookup.
/// </summary>
public sealed record DocumentSoftDeletedForAttachment(
    string AggregateId,
    string EntityType,
    string EntityId,
    string? Role) : DomainEvent(AggregateId);
