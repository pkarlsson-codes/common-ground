using CommonGround.Documents.Application.ReadModels;
using CommonGround.Documents.Domain.Documents;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Documents.Application.Queries;

/// <summary>Returns how to serve a version of a document (the current one unless specified).</summary>
public sealed record GetDocumentDownload(DocumentId DocumentId, string? VersionId = null);

/// <summary>A version's blob, with the file name and content type to serve it under.</summary>
public sealed record DocumentDownload(DocumentBlobDownload Blob, string FileName, string ContentType);

/// <summary>Handles <see cref="GetDocumentDownload"/>.</summary>
public static class GetDocumentDownloadHandler
{
    private static readonly TimeSpan PresignedExpiry = TimeSpan.FromMinutes(5);

    /// <summary>Handles the get document download request.</summary>
    public static async Task<DocumentDownload> Handle(
        GetDocumentDownload request,
        IQuerySession session,
        IDocumentBlobStore blobs,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var view = await session.LoadAsync<DocumentView>(request.DocumentId.Value, cancellationToken).ConfigureAwait(false)
            ?? throw new AggregateNotFoundException(request.DocumentId.Value);

        var versionId = request.VersionId ?? view.CurrentVersionId;
        var version = view.Versions.SingleOrDefault(v => v.VersionId == versionId)
            ?? throw new AggregateNotFoundException($"Version {versionId} of document {request.DocumentId.Value} was not found.");
        var blob = await blobs.GetDownloadAsync(version.BlobKey, PresignedExpiry, cancellationToken).ConfigureAwait(false);
        return new DocumentDownload(blob, version.FileName, version.ContentType);
    }
}
