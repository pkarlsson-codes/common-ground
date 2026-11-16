using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Documents.Domain.Documents.Events;

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
    string UploadedBy) : DomainEvent(AggregateId);
