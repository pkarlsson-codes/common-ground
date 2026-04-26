namespace CommonGround.Documents.Domain.Documents;

/// <summary>Abstraction over the blob backend (MinIO in production).</summary>
public interface IDocumentBlobStore
{
    /// <summary>Uploads a blob and returns the (key, sha256, sizeBytes) tuple.</summary>
    /// <remarks>The returned key is the input <paramref name="suggestedKey"/> verbatim.</remarks>
    Task<DocumentBlobUploadResult> UploadAsync(
        Stream content,
        string contentType,
        string suggestedKey,
        CancellationToken cancellationToken);

    /// <summary>Returns a presigned GET URL valid for the given expiry window.</summary>
    Task<Uri> GetPresignedDownloadUrlAsync(
        string blobKey,
        TimeSpan expiry,
        CancellationToken cancellationToken);
}

/// <summary>The result of an upload: the storage key, hash, and observed byte length.</summary>
public sealed record DocumentBlobUploadResult(string BlobKey, string Sha256, long SizeBytes);
