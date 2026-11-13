using System.Collections.Concurrent;
using System.Security.Cryptography;
using CommonGround.Documents.Domain.Documents;

namespace CommonGround.API.IntegrationTests.Documents;

/// <summary>In-memory fake of <see cref="IDocumentBlobStore"/> for integration tests.</summary>
internal sealed class InMemoryDocumentBlobStore : IDocumentBlobStore
{
    private readonly ConcurrentDictionary<string, byte[]> _store = new(StringComparer.Ordinal);

    public async Task<DocumentBlobUploadResult> UploadAsync(Stream content, string contentType, string suggestedKey, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        await content.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
        var bytes = ms.ToArray();
        _store[suggestedKey] = bytes;
        var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        return new DocumentBlobUploadResult(suggestedKey, hash, bytes.LongLength);
    }

    /// <summary>Serves the stored bytes, as the file system store does (the production default).</summary>
    public Task<DocumentBlobDownload> GetDownloadAsync(string blobKey, TimeSpan expiry, CancellationToken cancellationToken)
        => Task.FromResult<DocumentBlobDownload>(new DocumentBlobDownload.File(new MemoryStream(_store[blobKey], writable: false)));

    public Task DeleteAsync(string blobKey, CancellationToken cancellationToken)
    {
        _store.TryRemove(blobKey, out _);
        return Task.CompletedTask;
    }

    /// <summary>Gets whether a blob is stored under <paramref name="blobKey"/>.</summary>
    public bool Contains(string blobKey) => _store.ContainsKey(blobKey);
}
