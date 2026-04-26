using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Documents.Domain.Documents.Events;

/// <summary>Raised when a new version is added to an existing Document.</summary>
public sealed record DocumentVersionAdded(
    string AggregateId,
    string VersionId,
    int VersionNumber,
    string BlobKey,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Sha256,
    string? Note,
    string UploadedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
