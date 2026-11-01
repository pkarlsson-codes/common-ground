using CommonGround.Documents.Domain.Documents;
using CommonGround.Documents.Domain.Documents.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

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
    /// <summary>Updates the metadata, unless nothing changes.</summary>
    public static DocumentMetadataUpdated? Handle(UpdateDocumentMetadata command, [WriteAggregate] Document document)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(document);

        return document.UpdateMetadata(command.Title, command.Sensitivity, command.ExpiresOn, command.ClearExpiresOn, command.UpdatedBy);
    }
}
