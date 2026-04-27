using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Documents.Domain.Documents.Events;

/// <summary>Raised when a normalised tag is added to a Document.</summary>
public sealed record DocumentTagged(
    string AggregateId,
    string Tag,
    string TaggedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
