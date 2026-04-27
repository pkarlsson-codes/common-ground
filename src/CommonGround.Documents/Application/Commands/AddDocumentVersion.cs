using CommonGround.Documents.Domain.Documents;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

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
        IDocumentRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var doc = await repository.GetByIdAsync(command.DocumentId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.DocumentId.Value);

        doc.AddVersion(
            command.BlobKey,
            command.FileName,
            command.ContentType,
            command.SizeBytes,
            command.Sha256,
            command.Note,
            command.UploadedBy,
            command.VersionId);

        repository.Stage(doc);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return doc.CurrentVersionId;
    }
}
