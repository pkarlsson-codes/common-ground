namespace CommonGround.HomeBuyers.Infrastructure.Seeding;

/// <summary>Deterministic lookup from household seed keys to created aggregate IDs.</summary>
public sealed class HouseholdSeedLookup
{
    /// <summary>Gets or sets the deterministic document identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the stable seed key from households.seed.json.</summary>
    public string SeedKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the created household aggregate ID.</summary>
    public string HouseholdId { get; set; } = string.Empty;

    /// <summary>Gets or sets the created application aggregate ID, if the seed submits one.</summary>
    public string? ApplicationId { get; set; }

    /// <summary>Gets or sets when this mapping was last written.</summary>
    public DateTimeOffset SeededAt { get; set; }

    /// <summary>Creates a deterministic document ID for a household seed key.</summary>
    public static string BuildId(string seedKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seedKey);
        return $"household-seed:{seedKey}";
    }
}
