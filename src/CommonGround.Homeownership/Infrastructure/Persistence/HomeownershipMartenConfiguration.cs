using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Infrastructure.Persistence.Projections;
using JasperFx.Events.Projections;
using Marten;
using Weasel.Postgresql.Tables;

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
    }

    private static void ConfigureProjections(StoreOptions options)
    {
        options.Projections.Add<GroundLeaseViewProjection>(ProjectionLifecycle.Async);
        options.Projections.Add<GroundLeaseListItemViewProjection>(ProjectionLifecycle.Async);
        options.Projections.Add<PurchaseWorkflowViewProjection>(ProjectionLifecycle.Async);
        options.Projections.Add<PurchaseWorkflowListItemViewProjection>(ProjectionLifecycle.Async);
    }
}
