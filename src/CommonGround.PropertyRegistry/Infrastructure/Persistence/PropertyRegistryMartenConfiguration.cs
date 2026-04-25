using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Infrastructure.Persistence.Projections;
using CommonGround.PropertyRegistry.Infrastructure.Seeding;
using JasperFx.Events.Projections;
using Marten;
using Weasel.Postgresql;
using Weasel.Postgresql.Tables;

namespace CommonGround.PropertyRegistry.Infrastructure.Persistence;

/// <summary>Marten schema and projection configuration for the PropertyRegistry BC.</summary>
public static class PropertyRegistryMartenConfiguration
{
    private const string SchemaName = "property_registry";
    private const string SeedSchemaName = "property_registry_seed";

    /// <summary>Configures Marten for PropertyRegistry.</summary>
    public static void Configure(StoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Storage.ExtendedSchemaObjects.Add(new Extension("pg_trgm"));

        ConfigureReadModels(options);
        ConfigureProjections(options);
    }

    private static void ConfigureReadModels(StoreOptions options)
    {
        options.Schema.For<LandParcelView>().DatabaseSchemaName(SchemaName);
        options.Schema.For<LandParcelListItemView>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.Address)
            .Index(x => x.LegalParcelNumber);
        options.Schema.For<LandParcelSeedLookup>()
            .DatabaseSchemaName(SeedSchemaName)
            .Index(x => x.SeedKey);
        options.Schema.For<BuildingSeedLookup>()
            .DatabaseSchemaName(SeedSchemaName)
            .Index(x => x.SeedKey);
        options.Schema.For<BuildingView>().DatabaseSchemaName(SchemaName);
        options.Schema.For<BuildingListItemView>().DatabaseSchemaName(SchemaName);
    }

    private static void ConfigureProjections(StoreOptions options)
    {
        options.Projections.Add<LandParcelViewProjection>(ProjectionLifecycle.Async);
        options.Projections.Add<LandParcelListItemViewProjection>(ProjectionLifecycle.Async);
        options.Projections.Add<BuildingViewProjection>(ProjectionLifecycle.Async);
        options.Projections.Add<BuildingListItemViewProjection>(ProjectionLifecycle.Async);
    }
}
