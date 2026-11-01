using CommonGround.Documents.Domain.Documents;
using CommonGround.Documents.Domain.Documents.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Documents.Application.Commands;

/// <summary>Adds a tag (lowercased, deduped) to a document.</summary>
public sealed record TagDocument(DocumentId DocumentId, string Tag, UserId TaggedBy);

/// <summary>Handles <see cref="TagDocument"/>.</summary>
public static class TagDocumentHandler
{
    /// <summary>Tags the document, unless it already has the tag.</summary>
    public static DocumentTagged? Handle(TagDocument command, [WriteAggregate] Document document)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(document);

        return document.Tag(command.Tag, command.TaggedBy);
    }
}
