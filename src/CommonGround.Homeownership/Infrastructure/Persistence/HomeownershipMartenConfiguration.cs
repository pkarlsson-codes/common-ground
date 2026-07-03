using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Infrastructure.Persistence.Projections;
using JasperFx.Events.Projections;
using Marten;

namespace CommonGround.Homeownership.Infrastructure.Persistence;

/// <summary>Marten schema and projection configuration for the Homeownership bounded context.</summary>
public static class HomeownershipMartenConfiguration
{
    private const string SchemaName = "homeownership";

    private static ProjectionLifecycle ProjectionLifecycleForEnvironment =>
        string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Testing", StringComparison.OrdinalIgnoreCase)
            ? ProjectionLifecycle.Inline
            : ProjectionLifecycle.Async;

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
        options.Projections.Add<GroundLeaseViewProjection>(ProjectionLifecycleForEnvironment);
        options.Projections.Add<GroundLeaseListItemViewProjection>(ProjectionLifecycleForEnvironment);
        options.Projections.Add<PurchaseWorkflowViewProjection>(ProjectionLifecycleForEnvironment);
        options.Projections.Add<PurchaseWorkflowListItemViewProjection>(ProjectionLifecycleForEnvironment);
        options.Projections.Add<GroundLeaseTemplateListItemViewProjection>(ProjectionLifecycleForEnvironment);
        options.Projections.Add<GroundLeaseTemplateDetailViewProjection>(ProjectionLifecycleForEnvironment);
    }
}
