using CommonGround.Documents.Domain.Documents;
using CommonGround.SharedKernel.Domain;
using Marten;

namespace CommonGround.Documents.Application.Commands;

/// <summary>Uploads a brand-new document with its first version, optionally attaching it on creation.</summary>
/// <param name="Title">Human-readable title.</param>
/// <param name="Sensitivity">Sensitivity classification.</param>
/// <param name="Retention">Retention class.</param>
/// <param name="BlobKey">The MinIO object key produced by the uploader.</param>
/// <param name="FileName">Original filename as submitted.</param>
/// <param name="ContentType">MIME type.</param>
/// <param name="SizeBytes">Observed byte length.</param>
/// <param name="Sha256">Hex-encoded SHA-256 of the blob contents.</param>
/// <param name="UploadedBy">The staff user uploading.</param>
/// <param name="InitialAttachment">Optional entity to attach on creation.</param>
/// <param name="DocumentId">Optional pre-computed document ID (used for deterministic blob keys).</param>
/// <param name="VersionId">Optional pre-computed version ID (used for deterministic blob keys).</param>
public sealed record UploadDocument(
    string Title,
    SensitivityLevel Sensitivity,
    RetentionClass Retention,
    string BlobKey,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Sha256,
    UserId UploadedBy,
    UploadDocumentAttachment? InitialAttachment = null,
    DocumentId? DocumentId = null,
    DocumentVersionId? VersionId = null);

/// <summary>Initial attachment to apply when uploading a new document.</summary>
public sealed record UploadDocumentAttachment(string EntityType, string EntityId, string? Role);

/// <summary>Handles <see cref="UploadDocument"/>.</summary>
public static class UploadDocumentHandler
{
    /// <summary>Creates the aggregate and persists.</summary>
    public static Task<DocumentId> Handle(
        UploadDocument command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var doc = Document.Upload(
            command.Title,
            command.Sensitivity,
            command.Retention,
            command.BlobKey,
            command.FileName,
            command.ContentType,
            command.SizeBytes,
            command.Sha256,
            command.UploadedBy,
            command.InitialAttachment is { } a
                ? (a.EntityType, a.EntityId, a.Role)
                : null,
            command.DocumentId,
            command.VersionId);

        session.Events.StartStream<Document>(doc.Id.Value, doc.UncommittedEvents.ToArray());
        doc.UncommittedEvents.MarkAsCommitted();

        return Task.FromResult(doc.Id);
    }
}
