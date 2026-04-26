using CommonGround.Documents.Application.ReadModels;
using CommonGround.Documents.Domain.Documents;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Documents.Application.Queries;

/// <summary>Returns a single document by ID.</summary>
/// <param name="DocumentId">The document to fetch.</param>
public sealed record GetDocument(DocumentId DocumentId);

/// <summary>Handles <see cref="GetDocument"/>.</summary>
public static class GetDocumentHandler
{
    /// <summary>Handles the get document request.</summary>
    public static async Task<DocumentView> Handle(
        GetDocument request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var view = await session.LoadAsync<DocumentView>(request.DocumentId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.DocumentId.Value);

        return view;
    }
}
