using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Documents.Domain.Documents.Events;

public sealed record DocumentDetached(
    string AggregateId,
    string EntityType,
    string EntityId,
    string? Role,
    string DetachedBy) : DomainEvent(AggregateId);
