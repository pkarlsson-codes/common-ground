using CommonGround.Identity.Application.ReadModels;
using CommonGround.Identity.Infrastructure.Persistence.Projections;
using CommonGround.Identity.Infrastructure.Seeding;
using JasperFx.Events.Projections;
using Marten;

namespace CommonGround.Identity.Infrastructure.Persistence;

/// <summary>Marten schema configuration for the Identity BC.</summary>
public static class IdentityMartenConfiguration
{
    /// <summary>Configures Marten for Identity.</summary>
    public static void Configure(StoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Schema.For<UserView>()
            .DatabaseSchemaName("identity");

        options.Schema.For<UserSeedLookup>()
            .DatabaseSchemaName("identity");

        options.Projections.Add<UserViewProjection>(ProjectionLifecycle.Inline);
    }
}
