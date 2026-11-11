namespace CommonGround.Documents.Infrastructure.BlobStorage;

/// <summary>Where document files are kept. Set through <c>Documents:Store</c>.</summary>
public enum DocumentStore
{
    /// <summary>A local directory (<see cref="FileSystemBlobStoreOptions"/>). The default.</summary>
    FileSystem,

    /// <summary>An S3 bucket (<see cref="S3BlobStoreOptions"/>): MinIO, AWS S3, Backblaze B2, Cloudflare R2 and similar.</summary>
    S3,
}
