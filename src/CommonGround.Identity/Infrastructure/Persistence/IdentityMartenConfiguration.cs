using CommonGround.Identity.Infrastructure.Persistence.Projections;
using JasperFx.Events.Projections;
using Marten;

namespace CommonGround.Identity.Infrastructure.Persistence;

/// <summary>Marten schema configuration for the Identity BC.</summary>
public static class IdentityMartenConfiguration
{
    /// <summary>Determines projection lifecycle based on environment (Inline for tests, Async otherwise).</summary>
    private static ProjectionLifecycle ProjectionLifecycleForEnvironment =>
        string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Testing", StringComparison.OrdinalIgnoreCase)
            ? ProjectionLifecycle.Inline
            : ProjectionLifecycle.Async;

    /// <summary>Configures Marten for Identity.</summary>
    public static void Configure(StoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Schema.For<UserSubjectIndex>()
            .DatabaseSchemaName("identity")
            .SingleTenanted();

        options.Projections.Add<UserViewProjection>(ProjectionLifecycleForEnvironment);
    }
}
