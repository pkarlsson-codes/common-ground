using CommonGround.Documents.Application.ReadModels;
using CommonGround.SharedKernel.Application.WorkItems;

namespace CommonGround.Documents.Application.WorkItems;

/// <summary>Derives the work items for documents from the document library.</summary>
public static class DocumentsWorkItems
{
    /// <summary>How far ahead a document counts as expiring.</summary>
    public const int ExpiryWindowDays = 30;

    /// <summary>A document that expires soon, or has expired.</summary>
    public const string DocumentExpiring = "document-expiring";

    /// <summary>
    /// The item for a document that expires within <see cref="ExpiryWindowDays"/> of <paramref name="today"/>,
    /// or already has; none for a deleted document or one that doesn't expire soon.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="today">The trust's today.</param>
    /// <returns>The document's work item, or null when it isn't expiring.</returns>
    public static WorkItem? ForDocument(DocumentLibraryItemView document, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (document.IsDeleted || document.ExpiresOn is not { } expiresOn || expiresOn > today.AddDays(ExpiryWindowDays))
            return null;

        return new WorkItem(
            WorkArea.Records, DocumentExpiring, "document", document.Id, document.Title,
            expiresOn < today ? "Expired" : "Expiring", expiresOn, expiresOn.AddDays(-ExpiryWindowDays), []);
    }
}
