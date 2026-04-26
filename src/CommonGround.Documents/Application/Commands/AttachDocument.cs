using CommonGround.Documents.Domain.Documents;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

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
        IDocumentRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var doc = await repository.GetByIdAsync(command.DocumentId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.DocumentId.Value);

        doc.Attach(command.EntityType, command.EntityId, command.Role, command.AttachedBy);

        repository.Stage(doc);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
