using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Documents.Domain.Documents.Events;

/// <summary>Raised when a tag is removed from a Document.</summary>
public sealed record DocumentUntagged(
    string AggregateId,
    string Tag,
    string UntaggedBy) : DomainEvent(AggregateId);
