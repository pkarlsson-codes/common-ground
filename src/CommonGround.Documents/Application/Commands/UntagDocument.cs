using CommonGround.Documents.Domain.Documents;
using CommonGround.Documents.Domain.Documents.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Documents.Application.Commands;

/// <summary>Removes a tag from a document. Idempotent.</summary>
public sealed record UntagDocument(DocumentId DocumentId, string Tag, UserId UntaggedBy);

/// <summary>Handles <see cref="UntagDocument"/>.</summary>
public static class UntagDocumentHandler
{
    /// <summary>Untags the document, unless it doesn't have the tag.</summary>
    public static DocumentUntagged? Handle(UntagDocument command, [WriteAggregate] Document document)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(document);

        return document.Untag(command.Tag, command.UntaggedBy);
    }
}
