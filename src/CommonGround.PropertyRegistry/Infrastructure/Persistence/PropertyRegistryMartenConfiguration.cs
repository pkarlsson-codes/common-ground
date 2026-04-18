using Marten;

namespace CommonGround.PropertyRegistry.Infrastructure.Persistence;

/// <summary>Marten schema and projection configuration for the PropertyRegistry BC.</summary>
public static class PropertyRegistryMartenConfiguration
{
    private const string SchemaName = "property_registry";

    /// <summary>Configures Marten for PropertyRegistry.</summary>
    public static void Configure(StoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
    }
}
