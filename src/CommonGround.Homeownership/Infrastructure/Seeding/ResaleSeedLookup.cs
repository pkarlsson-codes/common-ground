namespace CommonGround.Homeownership.Infrastructure.Seeding;

/// <summary>Deterministic lookup from resale seed keys to the resale the seed opened.</summary>
public sealed class ResaleSeedLookup
{
    /// <summary>Gets or sets the deterministic document identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the stable seed key from resales.seed.json.</summary>
    public string SeedKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the resale the seed opened.</summary>
    public string ResaleId { get; set; } = string.Empty;

    /// <summary>Gets or sets when this mapping was last written.</summary>
    public DateTimeOffset SeededAt { get; set; }

    /// <summary>Creates a deterministic document ID for a resale seed key.</summary>
    public static string BuildId(string seedKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seedKey);
        return $"resale-seed:{seedKey}";
    }
}
