using CommonGround.SharedKernel.Domain;

namespace CommonGround.Documents.Domain.Documents;

/// <summary>A single uploaded version of a Document. Immutable.</summary>
public sealed record DocumentVersion(
    DocumentVersionId VersionId,
    int VersionNumber,
    string BlobKey,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Sha256,
    string? Note,
    UserId UploadedBy,
    DateTimeOffset UploadedAt);
