using Marten;

namespace CommonGround.Identity.Infrastructure.Persistence;

/// <summary>Marten schema configuration for the Identity BC.</summary>
public static class IdentityMartenConfiguration
{
    /// <summary>Configures Marten for Identity.</summary>
    public static void Configure(StoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Schema.For<UserSubjectIndex>()
            .DatabaseSchemaName("identity")
            .SingleTenanted();
    }
}
