using Marten;

namespace CommonGround.Stewardship.Infrastructure.Persistence;

/// <summary>Marten configuration for the Stewardship bounded context.</summary>
public static class StewardshipMartenConfiguration
{
    /// <summary>Configures Marten store options for the Stewardship bounded context.</summary>
    public static void Configure(StoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        // No custom serialization config needed — events use only primitives.
    }
}
