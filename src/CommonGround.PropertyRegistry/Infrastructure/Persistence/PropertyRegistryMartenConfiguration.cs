using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Infrastructure.Persistence.Projections;
using CommonGround.PropertyRegistry.Infrastructure.Seeding;
using JasperFx.Events.Projections;
using Marten;
using Weasel.Postgresql;

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
        options.Schema.For<HomeSeedLookup>()
            .DatabaseSchemaName(SeedSchemaName)
            .Index(x => x.SeedKey);
        options.Schema.For<HomeView>().DatabaseSchemaName(SchemaName);
        options.Schema.For<HomeListItemView>().DatabaseSchemaName(SchemaName);

        // Several handlers record news on the same home's availability; a version conflict retries the loser.
        options.Schema.For<HomeAvailabilityView>().DatabaseSchemaName(SchemaName).UseOptimisticConcurrency(true);
        options.Schema.For<HomeAssessmentView>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.UnitId);
        options.Schema.For<DevelopmentProjectListItemView>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.LandParcelId)
            .GinIndexJsonData();
    }

    private static void ConfigureProjections(StoreOptions options)
    {
        options.Projections.Add<LandParcelViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<LandParcelListItemViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<HomeViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<HomeListItemViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<HomeAssessmentViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<DevelopmentProjectListItemViewProjection>(ProjectionLifecycle.Inline);
    }
}
