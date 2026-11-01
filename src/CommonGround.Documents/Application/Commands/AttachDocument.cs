using CommonGround.Documents.Domain.Documents;
using CommonGround.Documents.Domain.Documents.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Documents.Application.Commands;

/// <summary>Attaches an existing document to an entity in another bounded context.</summary>
public sealed record AttachDocument(
    DocumentId DocumentId,
    string EntityType,
    string EntityId,
    string? Role,
    UserId AttachedBy);

/// <summary>Handles <see cref="AttachDocument"/>.</summary>
public static class AttachDocumentHandler
{
    /// <summary>Attaches the document, unless it is already attached with that role.</summary>
    public static DocumentAttached? Handle(AttachDocument command, [WriteAggregate] Document document)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(document);

        return document.Attach(command.EntityType, command.EntityId, command.Role, command.AttachedBy);
    }
}
