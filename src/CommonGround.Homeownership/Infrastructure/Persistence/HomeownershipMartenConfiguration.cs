using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Infrastructure.Persistence.Projections;
using CommonGround.Homeownership.Infrastructure.Seeding;
using JasperFx.Events.Projections;
using Marten;

namespace CommonGround.Homeownership.Infrastructure.Persistence;

/// <summary>Marten schema and projection configuration for the Homeownership bounded context.</summary>
public static class HomeownershipMartenConfiguration
{
    private const string SchemaName = "homeownership";
    private const string SeedSchemaName = "homeownership_seed";

    /// <summary>Configures Marten for Homeownership.</summary>
    public static void Configure(StoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        ConfigureReadModels(options);
        ConfigureProjections(options);
    }

    private static void ConfigureReadModels(StoreOptions options)
    {
        options.Schema.For<GroundLeaseView>().DatabaseSchemaName(SchemaName);
        options.Schema.For<GroundLeaseListItemView>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.HomeId)
            .Index(x => x.HomeownerFullName)
            .Index(x => x.HomeownerEmail);
        options.Schema.For<PurchaseWorkflowView>().DatabaseSchemaName(SchemaName);
        options.Schema.For<PurchaseWorkflowListItemView>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.BuyerFullName)
            .Index(x => x.HomeId)
            .Index(x => x.HouseholdId);
        options.Schema.For<ResaleView>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.GroundLeaseId)
            .Index(x => x.Status);
        options.Schema.For<MortgageRequestView>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.GroundLeaseId)
            .Index(x => x.Status);
        options.Schema.For<ImprovementRequestView>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.GroundLeaseId)
            .Index(x => x.Status);
        options.Schema.For<HeirTransferView>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.GroundLeaseId)
            .Index(x => x.Status);
        options.Schema.For<HomeLabelView>().DatabaseSchemaName(SchemaName);
        options.Schema.For<HomeSelectionView>()
            .DatabaseSchemaName(SchemaName)
            .UseOptimisticConcurrency(true)
            .Index(x => x.HomeId);
        options.Schema.For<GroundLeaseTemplateListItemView>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.TemplateName);
        options.Schema.For<GroundLeaseTemplateDetailView>().DatabaseSchemaName(SchemaName);
        options.Schema.For<GroundLeaseTemplateSeedLookup>()
            .DatabaseSchemaName(SeedSchemaName)
            .Index(x => x.SeedKey);
        options.Schema.For<PurchaseWorkflowSeedLookup>()
            .DatabaseSchemaName(SeedSchemaName)
            .Index(x => x.SeedKey);
        options.Schema.For<GroundLeaseSeedLookup>()
            .DatabaseSchemaName(SeedSchemaName)
            .Index(x => x.SeedKey);
        options.Schema.For<ResaleSeedLookup>()
            .DatabaseSchemaName(SeedSchemaName)
            .Index(x => x.SeedKey);
        options.Schema.For<ImprovementRequestSeedLookup>()
            .DatabaseSchemaName(SeedSchemaName)
            .Index(x => x.SeedKey);
        options.Schema.For<HeirTransferSeedLookup>()
            .DatabaseSchemaName(SeedSchemaName)
            .Index(x => x.SeedKey);
    }

    private static void ConfigureProjections(StoreOptions options)
    {
        options.Projections.Add<GroundLeaseViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<GroundLeaseListItemViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<PurchaseWorkflowViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<PurchaseWorkflowListItemViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<GroundLeaseTemplateListItemViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<GroundLeaseTemplateDetailViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<ResaleViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<MortgageRequestViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<ImprovementRequestViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<HeirTransferViewProjection>(ProjectionLifecycle.Inline);
    }
}
