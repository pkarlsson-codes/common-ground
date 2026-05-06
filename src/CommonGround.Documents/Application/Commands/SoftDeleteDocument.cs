using CommonGround.Documents.Domain.Documents;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Documents.Application.Commands;

/// <summary>Soft-deletes an existing document.</summary>
public sealed record SoftDeleteDocument(DocumentId DocumentId, string Reason, UserId DeletedBy);

/// <summary>Handles <see cref="SoftDeleteDocument"/>.</summary>
public static class SoftDeleteDocumentHandler
{
    /// <summary>Loads the document, soft-deletes it, and persists.</summary>
    public static async Task Handle(
        SoftDeleteDocument command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var stream = await session.Events
            .FetchForWriting<Document>(command.DocumentId.Value, cancellationToken)
            .ConfigureAwait(false);
        var doc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(Document), command.DocumentId.Value);

        doc.SoftDelete(command.Reason, command.DeletedBy);
        doc.ValidateInvariants();
        stream.AppendMany(doc.UncommittedEvents.ToArray());
        doc.UncommittedEvents.MarkAsCommitted();
    }
}
