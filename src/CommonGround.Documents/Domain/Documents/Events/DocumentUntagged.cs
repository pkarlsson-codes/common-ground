using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Documents.Domain.Documents.Events;

public sealed record DocumentUntagged(
    string AggregateId,
    string Tag,
    string UntaggedBy) : DomainEvent(AggregateId);
