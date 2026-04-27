using CommonGround.Documents.Domain.Documents;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Documents.Application.Commands;

/// <summary>Removes a tag from a document. Idempotent.</summary>
public sealed record UntagDocument(DocumentId DocumentId, string Tag, UserId UntaggedBy);

/// <summary>Handles <see cref="UntagDocument"/>.</summary>
public static class UntagDocumentHandler
{
    /// <summary>Loads the document, removes the tag, and persists.</summary>
    public static async Task Handle(
        UntagDocument command,
        IDocumentRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var doc = await repository.GetByIdAsync(command.DocumentId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.DocumentId.Value);

        doc.Untag(command.Tag, command.UntaggedBy);

        repository.Stage(doc);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
