namespace CommonGround.PropertyRegistry.Infrastructure.Seeding;

/// <summary>
/// Deterministic lookup from parcel seed keys to created aggregate IDs.
/// Future building seeders should query this document by <see cref="SeedKey"/> to resolve land parcel foreign keys.
/// </summary>
public sealed class LandParcelSeedLookup
{
    /// <summary>
    /// Gets or sets the deterministic document identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stable seed key from land-parcels.seed.json.
    /// </summary>
    public string SeedKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the created land parcel aggregate ID.
    /// </summary>
    public string LandParcelId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when this mapping was last written.
    /// </summary>
    public DateTimeOffset SeededAt { get; set; }

    /// <summary>
    /// Creates a deterministic document ID for a land parcel seed key.
    /// </summary>
    public static string BuildId(string seedKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seedKey);
        return $"land-parcel-seed:{seedKey}";
    }
}
