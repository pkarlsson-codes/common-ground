using CommonGround.Documents.Domain.Documents;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Documents.Application.Commands;

/// <summary>Adds a tag (lowercased, deduped) to a document.</summary>
public sealed record TagDocument(DocumentId DocumentId, string Tag, UserId TaggedBy);

/// <summary>Handles <see cref="TagDocument"/>.</summary>
public static class TagDocumentHandler
{
    /// <summary>Loads the document, applies the tag, and persists.</summary>
    public static async Task Handle(
        TagDocument command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var stream = await session.Events
            .FetchForWriting<Document>(command.DocumentId.Value, cancellationToken)
            .ConfigureAwait(false);
        var doc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(Document), command.DocumentId.Value);

        doc.Tag(command.Tag, command.TaggedBy);
        doc.ValidateInvariants();
        stream.AppendMany(doc.UncommittedEvents.ToArray());
        doc.UncommittedEvents.MarkAsCommitted();
    }
}
