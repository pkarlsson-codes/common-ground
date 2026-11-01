using CommonGround.Documents.Domain.Documents;
using CommonGround.Documents.Domain.Documents.Events;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Documents.Application.Commands;

/// <summary>Soft-deletes an existing document.</summary>
public sealed record SoftDeleteDocument(DocumentId DocumentId, string Reason, UserId DeletedBy);

/// <summary>Handles <see cref="SoftDeleteDocument"/>.</summary>
public static class SoftDeleteDocumentHandler
{
    /// <summary>Soft-deletes the document, unless it already is.</summary>
    public static IEnumerable<object> Handle(SoftDeleteDocument command, [WriteAggregate] Document document)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(document);

        return document.SoftDelete(command.Reason, command.DeletedBy);
    }
}
