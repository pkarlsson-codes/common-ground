using CommonGround.Documents.Domain.Documents;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Documents.Application.Commands;

/// <summary>Updates editable metadata on a document. Null fields leave the existing value untouched.</summary>
/// <param name="DocumentId">The document to update.</param>
/// <param name="Title">New title, or null to leave unchanged.</param>
/// <param name="Sensitivity">New sensitivity, or null to leave unchanged.</param>
/// <param name="ExpiresOn">New expiry, or null when <paramref name="ClearExpiresOn"/> is false to leave unchanged.</param>
/// <param name="ClearExpiresOn">When true, explicitly clears the expiry instead of leaving it.</param>
/// <param name="UpdatedBy">The staff user making the change.</param>
public sealed record UpdateDocumentMetadata(
    DocumentId DocumentId,
    string? Title,
    SensitivityLevel? Sensitivity,
    DateOnly? ExpiresOn,
    bool ClearExpiresOn,
    UserId UpdatedBy);

/// <summary>Handles <see cref="UpdateDocumentMetadata"/>.</summary>
public static class UpdateDocumentMetadataHandler
{
    /// <summary>Loads the document, updates metadata, and persists.</summary>
    public static async Task Handle(
        UpdateDocumentMetadata command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var stream = await session.Events
            .FetchForWriting<Document>(command.DocumentId.Value, cancellationToken)
            .ConfigureAwait(false);
        var doc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(Document), command.DocumentId.Value);

        doc.UpdateMetadata(command.Title, command.Sensitivity, command.ExpiresOn, command.ClearExpiresOn, command.UpdatedBy);
        doc.ValidateInvariants();
        stream.AppendMany(doc.UncommittedEvents.ToArray());
        doc.UncommittedEvents.MarkAsCommitted();
    }
}
