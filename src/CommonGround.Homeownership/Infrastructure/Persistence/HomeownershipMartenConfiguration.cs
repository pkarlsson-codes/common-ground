using Marten;

namespace CommonGround.Homeownership.Infrastructure.Persistence;

/// <summary>Marten serializer configuration for the Homeownership bounded context.</summary>
public static class HomeownershipMartenConfiguration
{
    /// <summary>Configures Marten for Homeownership.</summary>
    public static void Configure(StoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        // No custom serialization config needed — event uses only primitives.
    }
}
