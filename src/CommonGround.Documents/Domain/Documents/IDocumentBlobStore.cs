namespace CommonGround.Documents.Domain.Documents;

/// <summary>Abstraction over the blob backend: a local directory or an S3 bucket.</summary>
public interface IDocumentBlobStore
{
    /// <summary>Uploads a blob and returns the (key, sha256, sizeBytes) tuple.</summary>
    /// <remarks>The returned key is the input <paramref name="suggestedKey"/> verbatim.</remarks>
    Task<DocumentBlobUploadResult> UploadAsync(
        Stream content,
        string contentType,
        string suggestedKey,
        CancellationToken cancellationToken);

    /// <summary>Returns how to serve a blob: a URL valid for <paramref name="expiry"/> to redirect to, or the open file.</summary>
    Task<DocumentBlobDownload> GetDownloadAsync(
        string blobKey,
        TimeSpan expiry,
        CancellationToken cancellationToken);

    /// <summary>Deletes a blob. A key with no blob is not an error, so a delete that was cut short can run again.</summary>
    Task DeleteAsync(string blobKey, CancellationToken cancellationToken);
}

/// <summary>The result of an upload: the storage key, hash, and observed byte length.</summary>
public sealed record DocumentBlobUploadResult(string BlobKey, string Sha256, long SizeBytes);

/// <summary>How a blob is served: <see cref="Redirect"/> to a presigned URL, or <see cref="File"/> through the API.</summary>
public abstract record DocumentBlobDownload
{
    private DocumentBlobDownload()
    {
    }

    /// <summary>The client fetches the blob from <paramref name="Url"/> (an S3 presigned URL).</summary>
    public sealed record Redirect(Uri Url) : DocumentBlobDownload;

    /// <summary>The API streams <paramref name="Content"/> to the client. The caller disposes it.</summary>
    public sealed record File(Stream Content) : DocumentBlobDownload;
}
