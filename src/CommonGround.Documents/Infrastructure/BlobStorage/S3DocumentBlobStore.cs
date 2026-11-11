using System.Security.Cryptography;
using CommonGround.Documents.Domain.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace CommonGround.Documents.Infrastructure.BlobStorage;

/// <summary>S3 implementation (MinIO, AWS S3 or any S3-compatible service) of <see cref="IDocumentBlobStore"/>.</summary>
public sealed partial class S3DocumentBlobStore(
    IMinioClient client,
    IOptions<S3BlobStoreOptions> options,
    ILogger<S3DocumentBlobStore> logger)
    : IDocumentBlobStore
{
    private readonly S3BlobStoreOptions _options = options.Value;

    /// <inheritdoc/>
    public async Task<DocumentBlobUploadResult> UploadAsync(
        Stream content,
        string contentType,
        string suggestedKey,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(suggestedKey);

        // We need both SHA-256 and a known content length before PUT. Buffer to a temp file
        // (NOT memory) for arbitrarily large uploads; use the file as the upload source.
#pragma warning disable CA2007
        await using var temp = new FileStream(
            Path.Combine(Path.GetTempPath(), $"cg-doc-{Guid.NewGuid():N}.tmp"),
            FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None,
            bufferSize: 81920, FileOptions.DeleteOnClose | FileOptions.Asynchronous);
#pragma warning restore CA2007

        using var sha = SHA256.Create();
        var buffer = new byte[81920];
        int read;
        long total = 0;
        while ((read = await content.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
        {
            sha.TransformBlock(buffer, 0, read, null, 0);
            await temp.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
            total += read;
        }
        sha.TransformFinalBlock([], 0, 0);
        var hash = Convert.ToHexString(sha.Hash!).ToLowerInvariant();

        temp.Position = 0;

        var put = new PutObjectArgs()
            .WithBucket(_options.Bucket)
            .WithObject(suggestedKey)
            .WithStreamData(temp)
            .WithObjectSize(total)
            .WithContentType(contentType);

        await client.PutObjectAsync(put, cancellationToken).ConfigureAwait(false);

        Logs.UploadedBlob(logger, suggestedKey, total, hash);
        return new DocumentBlobUploadResult(suggestedKey, hash, total);
    }

    /// <inheritdoc/>
    public async Task<DocumentBlobDownload> GetDownloadAsync(
        string blobKey,
        TimeSpan expiry,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobKey);

        var args = new PresignedGetObjectArgs()
            .WithBucket(_options.Bucket)
            .WithObject(blobKey)
            .WithExpiry((int)expiry.TotalSeconds);

        var url = await client.PresignedGetObjectAsync(args).ConfigureAwait(false);
        _ = cancellationToken; // PresignedGetObjectAsync (Minio 6.x) does not accept a token.
        return new DocumentBlobDownload.Redirect(new Uri(url));
    }

    private static partial class Logs
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Uploaded blob {Key} ({Size} bytes, sha256={Hash})")]
        internal static partial void UploadedBlob(ILogger logger, string key, long size, string hash);
    }
}
