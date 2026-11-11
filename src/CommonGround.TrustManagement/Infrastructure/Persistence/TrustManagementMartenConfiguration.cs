using CommonGround.TrustManagement.Application.ReadModels;
using CommonGround.TrustManagement.Domain.Trusts.Events;
using CommonGround.TrustManagement.Infrastructure.Persistence.Projections;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using JasperFx.Events.Projections;
using Marten;

namespace CommonGround.TrustManagement.Infrastructure.Persistence;

/// <summary>Marten schema configuration for the TrustManagement BC.</summary>
public static class TrustManagementMartenConfiguration
{
    private const string SeedSchemaName = "trust_management_seed";

    /// <summary>Configures Marten for TrustManagement.</summary>
    public static void Configure(StoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Events.AddEventType<TrustCreated>();
        options.Schema.For<TrustSeedLookup>()
            .DatabaseSchemaName(SeedSchemaName)
            .Index(x => x.SeedKey);

        // Trusts and their invitations are written from the default-tenant session: a user who isn't in a
        // trust yet finds an invitation by its link.
        options.Schema.For<TrustInvitationView>()
            .DatabaseSchemaName("trust_management")
            .UniqueIndex(x => x.TokenHash)
            .Index(x => x.TrustId);

        options.Projections.Add<TrustInvitationViewProjection>(ProjectionLifecycle.Inline);
    }
}
