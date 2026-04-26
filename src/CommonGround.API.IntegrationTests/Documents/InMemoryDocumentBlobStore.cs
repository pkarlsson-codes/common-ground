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

    public Task<Uri> GetPresignedDownloadUrlAsync(string blobKey, TimeSpan expiry, CancellationToken cancellationToken)
        => Task.FromResult(new Uri($"https://fake-minio.local/{Uri.EscapeDataString(blobKey)}"));
}
