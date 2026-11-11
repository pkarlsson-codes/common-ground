using System.Security.Cryptography;
using CommonGround.Documents.Domain.Documents;
using Microsoft.Extensions.Logging;

namespace CommonGround.Documents.Infrastructure.BlobStorage;

/// <summary>Keeps each blob as a file under <c>rootPath</c>, at the path its key names.</summary>
/// <remarks>
/// A file is written to <c>.tmp</c> under the root first and moved into place when complete,
/// so a failed upload never leaves a partial file at a key.
/// </remarks>
public sealed partial class FileSystemDocumentBlobStore : IDocumentBlobStore
{
    private const int BufferSize = 81920;
    private readonly string _root;
    private readonly ILogger<FileSystemDocumentBlobStore> _logger;

    /// <summary>Initializes a new instance of the <see cref="FileSystemDocumentBlobStore"/> class.</summary>
    /// <param name="rootPath">The absolute directory that holds the files. It is created if missing.</param>
    /// <param name="logger">The logger.</param>
    public FileSystemDocumentBlobStore(string rootPath, ILogger<FileSystemDocumentBlobStore> logger)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
        _root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath)) + Path.DirectorySeparatorChar;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<DocumentBlobUploadResult> UploadAsync(
        Stream content,
        string contentType,
        string suggestedKey,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        var target = PathFor(suggestedKey);

        var tempDir = Path.Combine(_root, ".tmp");
        Directory.CreateDirectory(tempDir);
        var temp = Path.Combine(tempDir, $"{Guid.NewGuid():N}.tmp");

        string hash;
        long total = 0;
        try
        {
            using var sha = SHA256.Create();
            var file = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None, BufferSize, FileOptions.Asynchronous);
            await using (file.ConfigureAwait(false))
            {
                var buffer = new byte[BufferSize];
                int read;
                while ((read = await content.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    sha.TransformBlock(buffer, 0, read, null, 0);
                    await file.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                    total += read;
                }
            }

            sha.TransformFinalBlock([], 0, 0);
            hash = Convert.ToHexString(sha.Hash!).ToLowerInvariant();

            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Move(temp, target, overwrite: false);
        }
        finally
        {
            File.Delete(temp);
        }

        Logs.StoredBlob(_logger, suggestedKey, total, hash);
        return new DocumentBlobUploadResult(suggestedKey, hash, total);
    }

    /// <inheritdoc/>
    public Task<DocumentBlobDownload> GetDownloadAsync(string blobKey, TimeSpan expiry, CancellationToken cancellationToken)
    {
        var path = PathFor(blobKey);
        Stream content = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.Asynchronous);
        return Task.FromResult<DocumentBlobDownload>(new DocumentBlobDownload.File(content));
    }

    /// <summary>Maps a key such as <c>documents/{id}/{version}.pdf</c> to a path under the root, refusing keys that leave it.</summary>
    private string PathFor(string blobKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobKey);
        var path = Path.GetFullPath(Path.Combine(_root, blobKey));
        if (!path.StartsWith(_root, StringComparison.Ordinal) || path.StartsWith(Path.Combine(_root, ".tmp"), StringComparison.Ordinal))
        {
            throw new ArgumentException($"Blob key '{blobKey}' points outside the document store.", nameof(blobKey));
        }

        return path;
    }

    private static partial class Logs
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Stored blob {Key} ({Size} bytes, sha256={Hash})")]
        internal static partial void StoredBlob(ILogger logger, string key, long size, string hash);
    }
}
