using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Documents.Domain.Documents.Events;

/// <summary>Raised when a brand-new Document is created with its first version.</summary>
public sealed record DocumentUploaded(
    string AggregateId,
    string VersionId,
    string Title,
    SensitivityLevel Sensitivity,
    RetentionClass Retention,
    string BlobKey,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Sha256,
    string UploadedBy) : DomainEvent(AggregateId);
