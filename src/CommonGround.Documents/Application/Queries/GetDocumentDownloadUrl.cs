using CommonGround.Documents.Application.ReadModels;
using CommonGround.Documents.Domain.Documents;
using CommonGround.SharedKernel.Domain.Exceptions;
using Marten;

namespace CommonGround.Documents.Application.Queries;

/// <summary>Returns a short-lived presigned URL for a version of a document (the current one unless specified).</summary>
public sealed record GetDocumentDownloadUrl(DocumentId DocumentId, string? VersionId = null);

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

        var versionId = request.VersionId ?? view.CurrentVersionId;
        var version = view.Versions.SingleOrDefault(v => v.VersionId == versionId)
            ?? throw new AggregateNotFoundException($"Version {versionId} of document {request.DocumentId.Value} was not found.");
        return await blobs.GetPresignedDownloadUrlAsync(version.BlobKey, PresignedExpiry, cancellationToken).ConfigureAwait(false);
    }
}
