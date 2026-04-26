using CommonGround.Documents.Domain.Documents;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Documents.Application.Commands;

/// <summary>Soft-deletes an existing document.</summary>
public sealed record SoftDeleteDocument(DocumentId DocumentId, string Reason, UserId DeletedBy);

/// <summary>Handles <see cref="SoftDeleteDocument"/>.</summary>
public static class SoftDeleteDocumentHandler
{
    /// <summary>Loads the document, soft-deletes it, and persists.</summary>
    public static async Task Handle(
        SoftDeleteDocument command,
        IDocumentRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var doc = await repository.GetByIdAsync(command.DocumentId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.DocumentId.Value);

        doc.SoftDelete(command.Reason, command.DeletedBy);

        repository.Stage(doc);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
