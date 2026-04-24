using CommonGround.Stewardship.Infrastructure.Projections;
using JasperFx.Events.Projections;
using Marten;

namespace CommonGround.Stewardship.Infrastructure.Persistence;

/// <summary>Marten configuration for the Stewardship bounded context.</summary>
public static class StewardshipMartenConfiguration
{
    private const string SchemaName = "stewardship";

    /// <summary>Configures Marten store options for the Stewardship bounded context.</summary>
    public static void Configure(StoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Schema.For<StewardshipCaseView>().DatabaseSchemaName(SchemaName);
        options.Schema.For<ForeclosurePreventionCaseView>().DatabaseSchemaName(SchemaName);

        options.Projections.Add<StewardshipCaseProjection>(ProjectionLifecycle.Async);
        options.Projections.Add<ForeclosurePreventionCaseProjection>(ProjectionLifecycle.Async);
    }
}
