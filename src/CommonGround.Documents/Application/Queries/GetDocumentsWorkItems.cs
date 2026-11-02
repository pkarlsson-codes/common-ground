using CommonGround.Documents.Application.ReadModels;
using CommonGround.Documents.Application.WorkItems;
using CommonGround.SharedKernel.Application.WorkItems;
using Marten;

namespace CommonGround.Documents.Application.Queries;

/// <summary>Returns the documents' work items: those expiring soon or expired.</summary>
/// <param name="Today">The trust's today, against which expiry dates are judged.</param>
public sealed record GetDocumentsWorkItems(DateOnly Today);

/// <summary>Handles <see cref="GetDocumentsWorkItems"/>.</summary>
public static class GetDocumentsWorkItemsHandler
{
    /// <summary>Loads the documents expiring within the window and derives their work items.</summary>
    public static async Task<WorkItemList> Handle(GetDocumentsWorkItems request, IQuerySession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var horizon = request.Today.AddDays(DocumentsWorkItems.ExpiryWindowDays);
        var documents = await session.Query<DocumentLibraryItemView>()
            .Where(d => !d.IsDeleted && d.ExpiresOn != null && d.ExpiresOn <= horizon)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new WorkItemList([.. documents.Select(d => DocumentsWorkItems.ForDocument(d, request.Today)).OfType<WorkItem>()]);
    }
}
