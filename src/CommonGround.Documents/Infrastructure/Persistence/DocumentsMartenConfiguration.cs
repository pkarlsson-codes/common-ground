using CommonGround.Documents.Application.ReadModels;
using CommonGround.Documents.Infrastructure.Persistence.Projections;
using JasperFx.Events.Projections;
using Marten;

namespace CommonGround.Documents.Infrastructure.Persistence;

/// <summary>Marten schema and projection configuration for the Documents BC.</summary>
public static class DocumentsMartenConfiguration
{
    private const string SchemaName = "documents";

    /// <summary>Configures Marten for Documents.</summary>
    public static void Configure(StoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Schema.For<DocumentView>().DatabaseSchemaName(SchemaName);
        options.Schema.For<EntityDocumentsView>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.EntityType)
            .Index(x => x.EntityId);
        options.Schema.For<DocumentLibraryItemView>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.IsDeleted)
            .Index(x => x.UploadedAt);

        options.Projections.Add<DocumentViewProjection>(ProjectionLifecycle.Async);
        options.Projections.Add<EntityDocumentsViewProjection>(ProjectionLifecycle.Async);
        options.Projections.Add<DocumentLibraryItemViewProjection>(ProjectionLifecycle.Async);
    }
}
