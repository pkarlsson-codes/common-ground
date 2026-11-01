using CommonGround.Documents.Domain.Documents;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Documents.Application.Commands;

/// <summary>Uploads a brand-new document with its first version, optionally attaching it on creation.</summary>
/// <param name="DocumentId">The new document's ID, chosen by the caller (it is also part of the blob key).</param>
/// <param name="VersionId">The first version's ID, chosen by the caller (it is also part of the blob key).</param>
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
public sealed record UploadDocument(
    DocumentId DocumentId,
    DocumentVersionId VersionId,
    string Title,
    SensitivityLevel Sensitivity,
    RetentionClass Retention,
    string BlobKey,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Sha256,
    UserId UploadedBy,
    UploadDocumentAttachment? InitialAttachment = null);

/// <summary>Initial attachment to apply when uploading a new document.</summary>
public sealed record UploadDocumentAttachment(string EntityType, string EntityId, string? Role);

/// <summary>Handles <see cref="UploadDocument"/>.</summary>
public static class UploadDocumentHandler
{
    /// <summary>Starts the document's stream.</summary>
    public static IStartStream Handle(UploadDocument command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var events = Document.Upload(
            command.DocumentId,
            command.VersionId,
            command.Title,
            command.Sensitivity,
            command.Retention,
            command.BlobKey,
            command.FileName,
            command.ContentType,
            command.SizeBytes,
            command.Sha256,
            command.UploadedBy,
            command.InitialAttachment is { } a ? (a.EntityType, a.EntityId, a.Role) : null);

        return MartenOps.StartStream<Document>(command.DocumentId.Value, [.. events]);
    }
}
