using CommonGround.Documents.Domain.Documents;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Documents.Application.Commands;

/// <summary>Removes a tag from a document. Idempotent.</summary>
public sealed record UntagDocument(DocumentId DocumentId, string Tag, UserId UntaggedBy);

/// <summary>Handles <see cref="UntagDocument"/>.</summary>
public static class UntagDocumentHandler
{
    /// <summary>Loads the document, removes the tag, and persists.</summary>
    public static async Task Handle(
        UntagDocument command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var stream = await session.Events
            .FetchForWriting<Document>(command.DocumentId.Value, cancellationToken)
            .ConfigureAwait(false);
        var doc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(Document), command.DocumentId.Value);

        doc.Untag(command.Tag, command.UntaggedBy);
        doc.ValidateInvariants();
        stream.AppendMany(doc.UncommittedEvents.ToArray());
        doc.UncommittedEvents.MarkAsCommitted();
    }
}
