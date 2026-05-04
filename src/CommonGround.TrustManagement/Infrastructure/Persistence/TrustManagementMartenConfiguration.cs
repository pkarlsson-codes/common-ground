using CommonGround.TrustManagement.Domain.Trusts.Events;
using CommonGround.TrustManagement.Infrastructure.Seeding;
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
    }
}
