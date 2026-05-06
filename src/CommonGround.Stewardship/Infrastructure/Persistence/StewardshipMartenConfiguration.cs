using CommonGround.Stewardship.Infrastructure.Projections;
using JasperFx.Events.Projections;
using Marten;

namespace CommonGround.Stewardship.Infrastructure.Persistence;

/// <summary>Marten configuration for the Stewardship bounded context.</summary>
public static class StewardshipMartenConfiguration
{
    private const string SchemaName = "stewardship";

    private static ProjectionLifecycle ProjectionLifecycleForEnvironment =>
        string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Testing", StringComparison.OrdinalIgnoreCase)
            ? ProjectionLifecycle.Inline
            : ProjectionLifecycle.Async;

    /// <summary>Configures Marten store options for the Stewardship bounded context.</summary>
    public static void Configure(StoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Schema.For<StewardshipCaseView>().DatabaseSchemaName(SchemaName);
        options.Schema.For<ForeclosurePreventionCaseView>().DatabaseSchemaName(SchemaName);

        options.Projections.Add<StewardshipCaseProjection>(ProjectionLifecycleForEnvironment);
        options.Projections.Add<ForeclosurePreventionCaseProjection>(ProjectionLifecycleForEnvironment);
    }
}
