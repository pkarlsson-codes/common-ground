using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Infrastructure.Persistence.Projections;
using JasperFx.Events.Projections;
using Marten;

namespace CommonGround.Homeownership.Infrastructure.Persistence;

/// <summary>Marten schema and projection configuration for the Homeownership bounded context.</summary>
public static class HomeownershipMartenConfiguration
{
    private const string SchemaName = "homeownership";

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
            .Index(x => x.HomeownerFullName)
            .Index(x => x.HomeownerEmail);
        options.Schema.For<PurchaseWorkflowView>().DatabaseSchemaName(SchemaName);
        options.Schema.For<PurchaseWorkflowListItemView>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.BuyerFullName);
        options.Schema.For<GroundLeaseTemplateListItemView>()
            .DatabaseSchemaName(SchemaName)
            .Index(x => x.TemplateName);
        options.Schema.For<GroundLeaseTemplateDetailView>().DatabaseSchemaName(SchemaName);
    }

    private static void ConfigureProjections(StoreOptions options)
    {
        options.Projections.Add<GroundLeaseViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<GroundLeaseListItemViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<PurchaseWorkflowViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<PurchaseWorkflowListItemViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<GroundLeaseTemplateListItemViewProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<GroundLeaseTemplateDetailViewProjection>(ProjectionLifecycle.Inline);
    }
}
