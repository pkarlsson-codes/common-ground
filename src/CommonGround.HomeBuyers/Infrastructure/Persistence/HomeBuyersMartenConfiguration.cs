using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Infrastructure.Persistence.Projections;
using CommonGround.HomeBuyers.Infrastructure.Seeding;
using JasperFx.Events.Projections;
using Marten;
using Marten.Schema.Indexing.Unique;

namespace CommonGround.HomeBuyers.Infrastructure.Persistence;

/// <summary>Marten schema and projection configuration for the HomeBuyers bounded context.</summary>
public static class HomeBuyersMartenConfiguration
{
    private const string SchemaName = "home_buyers";
    private const string SeedSchemaName = "home_buyers_seed";

    /// <summary>Configures Marten for HomeBuyers.</summary>
    public static void Configure(StoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        ConfigureReadModels(options);
        ConfigureProjections(options);
    }

    private static void ConfigureReadModels(StoreOptions options)
    {
        options.Schema.For<HomeownerApplicationView>().DatabaseSchemaName(SchemaName);
        options.Schema.For<HomeownerApplicationListItemView>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.HouseholdId)
            .Index(x => x.Phase);
        // Choosing a household marks the home's facts as chosen. Optimistic concurrency makes the second of two
        // choices racing for the same home fail and retry, and the retry finds the home no longer offered.
        options.Schema.For<HomeFactsView>()
            .DatabaseSchemaName(SchemaName)
            .UseOptimisticConcurrency(true);
        options.Schema.For<HomeSelectionView>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.HomeId);
        // One schedule per effective date. IncomeLimitSchedule.Enter and Correct check this from the
        // view; the unique index catches two entries racing past that check.
        options.Schema.For<IncomeLimitScheduleView>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.EffectiveFrom, index =>
            {
                index.IsUnique = true;
                index.TenancyScope = TenancyScope.PerTenant;
            });
        options.Schema.For<HouseholdView>().DatabaseSchemaName(SchemaName);
        options.Schema.For<HouseholdListItemView>().DatabaseSchemaName(SchemaName);
        options.Schema.For<HouseholdPassOversView>().DatabaseSchemaName(SchemaName);
        options.Schema.For<HouseholdSeedLookup>()
            .DatabaseSchemaName(SeedSchemaName)
            .Index(x => x.SeedKey);
        options.Schema.For<HomeSelectionSeedLookup>()
            .DatabaseSchemaName(SeedSchemaName)
            .Index(x => x.SeedKey);
    }

    private static void ConfigureProjections(StoreOptions options)
    {
        options.Projections.Add<HomeownerApplicationViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<HomeownerApplicationListItemViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<HomeSelectionViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<IncomeLimitScheduleViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<HouseholdViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<HouseholdListItemViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<HouseholdPassOversViewProjection>(ProjectionLifecycle.Inline);
    }
}