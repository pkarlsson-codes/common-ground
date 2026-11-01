using CommonGround.Documents.Domain.Documents;
using CommonGround.Documents.Domain.Documents.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Documents.Application.Commands;

/// <summary>Adds a new version to an existing document.</summary>
/// <param name="DocumentId">The document to extend.</param>
/// <param name="VersionId">The new version's ID, chosen by the caller (it is also part of the blob key).</param>
/// <param name="BlobKey">The MinIO object key produced by the uploader.</param>
/// <param name="FileName">Original filename as submitted.</param>
/// <param name="ContentType">MIME type.</param>
/// <param name="SizeBytes">Observed byte length.</param>
/// <param name="Sha256">Hex-encoded SHA-256 of the blob contents.</param>
/// <param name="Note">Optional version note.</param>
/// <param name="UploadedBy">The staff user uploading.</param>
public sealed record AddDocumentVersion(
    DocumentId DocumentId,
    DocumentVersionId VersionId,
    string BlobKey,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Sha256,
    string? Note,
    UserId UploadedBy);

/// <summary>Handles <see cref="AddDocumentVersion"/>.</summary>
public static class AddDocumentVersionHandler
{
    /// <summary>Adds the version to the document.</summary>
    public static DocumentVersionAdded Handle(AddDocumentVersion command, [WriteAggregate] Document document)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(document);

        return document.AddVersion(
            command.VersionId,
            command.BlobKey,
            command.FileName,
            command.ContentType,
            command.SizeBytes,
            command.Sha256,
            command.Note,
            command.UploadedBy);
    }
}
