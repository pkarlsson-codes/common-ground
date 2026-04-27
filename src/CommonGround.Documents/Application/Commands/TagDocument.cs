using CommonGround.Documents.Domain.Documents;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Documents.Application.Commands;

/// <summary>Adds a tag (lowercased, deduped) to a document.</summary>
public sealed record TagDocument(DocumentId DocumentId, string Tag, UserId TaggedBy);

/// <summary>Handles <see cref="TagDocument"/>.</summary>
public static class TagDocumentHandler
{
    /// <summary>Loads the document, applies the tag, and persists.</summary>
    public static async Task Handle(
        TagDocument command,
        IDocumentRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var doc = await repository.GetByIdAsync(command.DocumentId, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(command.DocumentId.Value);

        doc.Tag(command.Tag, command.TaggedBy);

        repository.Stage(doc);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
