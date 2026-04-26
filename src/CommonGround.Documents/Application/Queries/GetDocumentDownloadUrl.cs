using CommonGround.Documents.Application.ReadModels;
using CommonGround.Documents.Domain.Documents;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Documents.Application.Queries;

/// <summary>Returns a short-lived presigned URL for the current version of a document.</summary>
public sealed record GetDocumentDownloadUrl(DocumentId DocumentId);

/// <summary>Handles <see cref="GetDocumentDownloadUrl"/>.</summary>
public static class GetDocumentDownloadUrlHandler
{
    private static readonly TimeSpan PresignedExpiry = TimeSpan.FromMinutes(5);

    /// <summary>Handles the get document download URL request.</summary>
    public static async Task<Uri> Handle(
        GetDocumentDownloadUrl request,
        IQuerySession session,
        IDocumentBlobStore blobs,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var view = await session.LoadAsync<DocumentView>(request.DocumentId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.DocumentId.Value);

        var current = view.Versions.Single(v => v.VersionId == view.CurrentVersionId);
        return await blobs.GetPresignedDownloadUrlAsync(current.BlobKey, PresignedExpiry, cancellationToken).ConfigureAwait(false);
    }
}
