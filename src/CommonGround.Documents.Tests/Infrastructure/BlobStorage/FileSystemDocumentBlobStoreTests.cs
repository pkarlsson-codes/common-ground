using System.Security.Cryptography;
using System.Text;
using CommonGround.Documents.Domain.Documents;
using CommonGround.Documents.Infrastructure.BlobStorage;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CommonGround.Documents.Tests.Infrastructure.BlobStorage;

public class FileSystemDocumentBlobStoreTests
{
    public sealed class TestBed : IDisposable
    {
        public TestBed()
        {
            Sut = new FileSystemDocumentBlobStore(Root, NullLogger<FileSystemDocumentBlobStore>.Instance);
        }

        public string Root { get; } = Path.Combine(Path.GetTempPath(), $"cg-blobs-{Guid.NewGuid():N}");

        public FileSystemDocumentBlobStore Sut { get; }

        public Task<DocumentBlobUploadResult> Upload(string key, string text) =>
            Sut.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(text)), "text/plain", key, CancellationToken.None);

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
    }

    public sealed class TheUploadAsyncMethod : IDisposable
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public async Task GivenContent_WhenUploaded_ThenWritesTheFileAtItsKeyAndReturnsHashAndSize()
        {
            var result = await _testBed.Upload("documents/doc-1/ver-1.txt", "hello");

            File.ReadAllText(Path.Combine(_testBed.Root, "documents", "doc-1", "ver-1.txt")).Should().Be("hello");
            result.Should().Be(new DocumentBlobUploadResult(
                "documents/doc-1/ver-1.txt",
                Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("hello"))).ToLowerInvariant(),
                5));
        }

        [Fact]
        public async Task GivenUpload_WhenDone_ThenLeavesNoTempFile()
        {
            await _testBed.Upload("documents/doc-1/ver-1.txt", "hello");

            Directory.GetFiles(Path.Combine(_testBed.Root, ".tmp")).Should().BeEmpty();
        }

        [Fact]
        public async Task GivenExistingKey_WhenUploadedAgain_ThenThrowsAndKeepsTheFirstFile()
        {
            await _testBed.Upload("documents/doc-1/ver-1.txt", "first");

            var act = () => _testBed.Upload("documents/doc-1/ver-1.txt", "second");

            await act.Should().ThrowAsync<IOException>();
            File.ReadAllText(Path.Combine(_testBed.Root, "documents", "doc-1", "ver-1.txt")).Should().Be("first");
        }

        [Theory]
        [InlineData("../outside.txt")]
        [InlineData("documents/../../outside.txt")]
        [InlineData(".tmp/planted.tmp")]
        public async Task GivenKeyOutsideTheStore_WhenUploaded_ThenThrows(string key)
        {
            var act = () => _testBed.Upload(key, "x");

            await act.Should().ThrowAsync<ArgumentException>();
        }

        public void Dispose() => _testBed.Dispose();
    }

    public sealed class TheGetDownloadAsyncMethod : IDisposable
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public async Task GivenUploadedBlob_WhenDownloaded_ThenReturnsTheFileContent()
        {
            await _testBed.Upload("documents/doc-1/ver-1.txt", "hello");

            var download = await _testBed.Sut.GetDownloadAsync("documents/doc-1/ver-1.txt", TimeSpan.FromMinutes(5), CancellationToken.None);

            var file = download.Should().BeOfType<DocumentBlobDownload.File>().Subject;
            using var reader = new StreamReader(file.Content);
            (await reader.ReadToEndAsync()).Should().Be("hello");
        }

        [Fact]
        public async Task GivenKeyOutsideTheStore_WhenDownloaded_ThenThrows()
        {
            var act = () => _testBed.Sut.GetDownloadAsync("../secret.txt", TimeSpan.FromMinutes(5), CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>();
        }

        public void Dispose() => _testBed.Dispose();
    }

    public sealed class TheDeleteAsyncMethod : IDisposable
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public async Task GivenTheDocumentsLastVersion_WhenDeleted_ThenRemovesTheFileAndTheDocumentsFolder()
        {
            await _testBed.Upload("documents/doc-1/ver-1.txt", "hello");

            await _testBed.Sut.DeleteAsync("documents/doc-1/ver-1.txt", CancellationToken.None);

            Directory.Exists(Path.Combine(_testBed.Root, "documents", "doc-1")).Should().BeFalse();
            Directory.Exists(Path.Combine(_testBed.Root, "documents")).Should().BeTrue();
        }

        [Fact]
        public async Task GivenAnotherVersion_WhenOneIsDeleted_ThenKeepsTheOther()
        {
            await _testBed.Upload("documents/doc-1/ver-1.txt", "first");
            await _testBed.Upload("documents/doc-1/ver-2.txt", "second");

            await _testBed.Sut.DeleteAsync("documents/doc-1/ver-1.txt", CancellationToken.None);

            File.Exists(Path.Combine(_testBed.Root, "documents", "doc-1", "ver-2.txt")).Should().BeTrue();
        }

        [Fact]
        public async Task GivenNoBlobAtTheKey_WhenDeleted_ThenDoesNotThrow()
        {
            var act = () => _testBed.Sut.DeleteAsync("documents/doc-1/ver-1.txt", CancellationToken.None);

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GivenKeyOutsideTheStore_WhenDeleted_ThenThrows()
        {
            var act = () => _testBed.Sut.DeleteAsync("../secret.txt", CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>();
        }

        public void Dispose() => _testBed.Dispose();
    }
}
