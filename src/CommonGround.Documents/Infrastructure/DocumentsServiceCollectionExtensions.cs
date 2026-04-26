using CommonGround.Documents.Domain.Documents;
using CommonGround.Documents.Infrastructure.BlobStorage;
using CommonGround.Documents.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace CommonGround.Documents.Infrastructure;

/// <summary>DI registration for the Documents bounded context.</summary>
public static class DocumentsServiceCollectionExtensions
{
    /// <summary>Adds the Documents BC services (repository, blob store, MinIO client, options).</summary>
    public static IServiceCollection AddDocuments(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<MinIOOptions>(configuration.GetSection(MinIOOptions.SectionName));

        services.AddSingleton<IMinioClient>(sp =>
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MinIOOptions>>().Value;
            var client = new MinioClient()
                .WithEndpoint(opts.Endpoint)
                .WithCredentials(opts.AccessKey, opts.SecretKey);
            if (opts.UseSsl) client = client.WithSSL();
            return client.Build();
        });

        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IDocumentBlobStore, MinIODocumentBlobStore>();

        services.AddHostedService<MinIOBucketBootstrapper>();

        return services;
    }
}
