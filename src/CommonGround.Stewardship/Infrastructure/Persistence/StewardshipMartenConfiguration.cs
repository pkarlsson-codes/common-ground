using CommonGround.Stewardship.Infrastructure.Projections;
using CommonGround.Stewardship.Infrastructure.Seeding;
using JasperFx.Events.Projections;
using Marten;

namespace CommonGround.Stewardship.Infrastructure.Persistence;

/// <summary>Marten configuration for the Stewardship bounded context.</summary>
public static class StewardshipMartenConfiguration
{
    private const string SchemaName = "stewardship";
    private const string SeedSchemaName = "stewardship_seed";

    /// <summary>Configures Marten store options for the Stewardship bounded context.</summary>
    public static void Configure(StoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Schema.For<StewardshipCaseView>().DatabaseSchemaName(SchemaName);
        options.Schema.For<ForeclosurePreventionCaseView>().DatabaseSchemaName(SchemaName);
        options.Schema.For<StewardshipCaseSeedLookup>()
            .DatabaseSchemaName(SeedSchemaName)
            .Index(x => x.SeedKey);

        options.Projections.Add<StewardshipCaseProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<ForeclosurePreventionCaseProjection>(ProjectionLifecycle.Inline);
    }
}
