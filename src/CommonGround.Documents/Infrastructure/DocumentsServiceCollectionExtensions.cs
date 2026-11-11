using CommonGround.Documents.Domain.Documents;
using CommonGround.Documents.Infrastructure.BlobStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;

namespace CommonGround.Documents.Infrastructure;

/// <summary>DI registration for the Documents bounded context.</summary>
public static class DocumentsServiceCollectionExtensions
{
    /// <summary>The configuration key that picks the blob store (<see cref="DocumentStore"/>).</summary>
    public const string StoreKey = "Documents:Store";

    /// <summary>Adds the Documents BC services: the blob store that <c>Documents:Store</c> names, and its options.</summary>
    public static IServiceCollection AddDocuments(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var store = configuration.GetValue(StoreKey, DocumentStore.FileSystem);
        switch (store)
        {
            case DocumentStore.FileSystem:
                AddFileSystemStore(services, configuration, environment);
                break;
            case DocumentStore.S3:
                AddS3Store(services, configuration, environment);
                break;
            default:
                throw new InvalidOperationException($"{StoreKey} '{store}' is not supported. Use FileSystem or S3.");
        }

        return services;
    }

    private static void AddFileSystemStore(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.Configure<FileSystemBlobStoreOptions>(configuration.GetSection(FileSystemBlobStoreOptions.SectionName));
        services.AddSingleton<IDocumentBlobStore>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<FileSystemBlobStoreOptions>>().Value;
            var root = Path.Combine(environment.ContentRootPath, opts.RootPath);
            return new FileSystemDocumentBlobStore(root, sp.GetRequiredService<ILogger<FileSystemDocumentBlobStore>>());
        });
    }

    private static void AddS3Store(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.Configure<S3BlobStoreOptions>(configuration.GetSection(S3BlobStoreOptions.SectionName));

        services.AddSingleton<IMinioClient>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<S3BlobStoreOptions>>().Value;
            var client = new MinioClient()
                .WithEndpoint(opts.Endpoint)
                .WithCredentials(opts.AccessKey, opts.SecretKey);
            if (opts.UseSsl) client = client.WithSSL();
            return client.Build();
        });

        services.AddScoped<IDocumentBlobStore, S3DocumentBlobStore>();

        if (!environment.IsEnvironment("Testing"))
        {
            services.AddHostedService<S3BucketBootstrapper>();
        }
    }
}
