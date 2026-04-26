using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace CommonGround.Documents.Infrastructure.BlobStorage;

/// <summary>
/// Hosted service that ensures the documents bucket exists at startup.
/// </summary>
internal sealed partial class MinIOBucketBootstrapper(
    IMinioClient client,
    IOptions<MinIOOptions> options,
    ILogger<MinIOBucketBootstrapper> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var bucket = options.Value.Bucket;
        var exists = await client.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(bucket), cancellationToken).ConfigureAwait(false);

        if (!exists)
        {
            Logs.CreatingBucket(logger, bucket);
            await client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket), cancellationToken).ConfigureAwait(false);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static partial class Logs
    {
        [LoggerMessage(0, LogLevel.Information, "Creating MinIO bucket {Bucket}")]
        public static partial void CreatingBucket(ILogger logger, string bucket);
    }
}
