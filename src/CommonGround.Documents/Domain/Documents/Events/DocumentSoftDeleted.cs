using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Documents.Domain.Documents.Events;

/// <summary>Raised when a Document is soft-deleted (hidden from default listings).</summary>
public sealed record DocumentSoftDeleted(
    string AggregateId,
    string Reason,
    string DeletedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
