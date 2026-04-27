using CommonGround.Documents.Domain.Documents;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Documents.Application.Commands;

/// <summary>Updates editable metadata on a document. Null fields leave the existing value untouched.</summary>
/// <param name="DocumentId">The document to update.</param>
/// <param name="Title">New title, or null to leave unchanged.</param>
/// <param name="Sensitivity">New sensitivity, or null to leave unchanged.</param>
/// <param name="ExpiresAt">New expiry, or null when <paramref name="ClearExpiresAt"/> is false to leave unchanged.</param>
/// <param name="ClearExpiresAt">When true, explicitly clears the expiry instead of leaving it.</param>
/// <param name="UpdatedBy">The staff user making the change.</param>
public sealed record UpdateDocumentMetadata(
    DocumentId DocumentId,
    string? Title,
    SensitivityLevel? Sensitivity,
    DateTimeOffset? ExpiresAt,
    bool ClearExpiresAt,
    UserId UpdatedBy);

/// <summary>Handles <see cref="UpdateDocumentMetadata"/>.</summary>
public static class UpdateDocumentMetadataHandler
{
    /// <summary>Loads the document, updates metadata, and persists.</summary>
    public static async Task Handle(
        UpdateDocumentMetadata command,
        IDocumentRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var doc = await repository.GetByIdAsync(command.DocumentId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.DocumentId.Value);

        doc.UpdateMetadata(command.Title, command.Sensitivity, command.ExpiresAt, command.ClearExpiresAt, command.UpdatedBy);

        repository.Stage(doc);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
