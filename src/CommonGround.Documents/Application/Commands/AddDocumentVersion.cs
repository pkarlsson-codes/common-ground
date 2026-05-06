using CommonGround.Documents.Domain.Documents;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Documents.Application.Commands;

/// <summary>Adds a new version to an existing document.</summary>
/// <param name="DocumentId">The document to extend.</param>
/// <param name="BlobKey">The MinIO object key produced by the uploader.</param>
/// <param name="FileName">Original filename as submitted.</param>
/// <param name="ContentType">MIME type.</param>
/// <param name="SizeBytes">Observed byte length.</param>
/// <param name="Sha256">Hex-encoded SHA-256 of the blob contents.</param>
/// <param name="Note">Optional version note.</param>
/// <param name="UploadedBy">The staff user uploading.</param>
/// <param name="VersionId">Optional pre-computed version ID (used for deterministic blob keys).</param>
public sealed record AddDocumentVersion(
    DocumentId DocumentId,
    string BlobKey,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Sha256,
    string? Note,
    UserId UploadedBy,
    DocumentVersionId? VersionId = null);

/// <summary>Handles <see cref="AddDocumentVersion"/>.</summary>
public static class AddDocumentVersionHandler
{
    /// <summary>Loads the document, appends a version, and persists.</summary>
    public static async Task<DocumentVersionId> Handle(
        AddDocumentVersion command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var stream = await session.Events
            .FetchForWriting<Document>(command.DocumentId.Value, cancellationToken)
            .ConfigureAwait(false);
        var doc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(Document), command.DocumentId.Value);

        doc.AddVersion(
            command.BlobKey,
            command.FileName,
            command.ContentType,
            command.SizeBytes,
            command.Sha256,
            command.Note,
            command.UploadedBy,
            command.VersionId);
        doc.ValidateInvariants();
        stream.AppendMany(doc.UncommittedEvents.ToArray());
        doc.UncommittedEvents.MarkAsCommitted();

        return doc.CurrentVersionId;
    }
}
