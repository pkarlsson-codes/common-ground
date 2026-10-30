using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Infrastructure.Persistence.Projections;
using JasperFx.Events.Projections;
using Marten;
using Marten.Schema.Indexing.Unique;

namespace CommonGround.HomeBuyers.Infrastructure.Persistence;

/// <summary>Marten schema and projection configuration for the HomeBuyers bounded context.</summary>
public static class HomeBuyersMartenConfiguration
{
    private const string SchemaName = "home_buyers";

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
        options.Schema.For<HomeFactsView>().DatabaseSchemaName(SchemaName);
        // One open or completed selection per home. HomeSelection.Start checks this from the
        // view; the partial unique index catches two starts racing past that check.
        options.Schema.For<HomeSelectionView>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.HomeId, index =>
            {
                index.IsUnique = true;
                index.TenancyScope = TenancyScope.PerTenant;
                index.Predicate = $"data ->> 'Status' <> '{HomeSelectionStatus.Cancelled.DisplayName}'";
            });
        options.Schema.For<HouseholdView>().DatabaseSchemaName(SchemaName);
        options.Schema.For<HouseholdListItemView>().DatabaseSchemaName(SchemaName);
        options.Schema.For<HouseholdPassOversView>().DatabaseSchemaName(SchemaName);
    }

    private static void ConfigureProjections(StoreOptions options)
    {
        options.Projections.Add<HomeownerApplicationViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<HomeownerApplicationListItemViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<HomeSelectionViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<HouseholdViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<HouseholdListItemViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<HouseholdPassOversViewProjection>(ProjectionLifecycle.Inline);
    }
}