using CommonGround.Documents.Domain.Documents;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Documents.Application.Commands;

/// <summary>Attaches an existing document to an entity in another bounded context.</summary>
public sealed record AttachDocument(
    DocumentId DocumentId,
    string EntityType,
    Guid EntityId,
    string? Role,
    UserId AttachedBy);

/// <summary>Handles <see cref="AttachDocument"/>.</summary>
public static class AttachDocumentHandler
{
    /// <summary>Loads the document, attaches it, and persists.</summary>
    public static async Task Handle(
        AttachDocument command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var stream = await session.Events
            .FetchForWriting<Document>(command.DocumentId.Value, cancellationToken)
            .ConfigureAwait(false);
        var doc = stream.Aggregate ?? throw new AggregateNotFoundException(typeof(Document), command.DocumentId.Value);

        doc.Attach(command.EntityType, command.EntityId, command.Role, command.AttachedBy);
        doc.ValidateInvariants();
        stream.AppendMany(doc.UncommittedEvents.ToArray());
        doc.UncommittedEvents.MarkAsCommitted();
    }
}
