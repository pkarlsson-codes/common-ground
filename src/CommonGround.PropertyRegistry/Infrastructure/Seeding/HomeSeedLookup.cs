namespace CommonGround.PropertyRegistry.Infrastructure.Seeding;

/// <summary>
/// Deterministic lookup from home seed keys to created aggregate IDs.
/// </summary>
public sealed class HomeSeedLookup
{
    /// <summary>
    /// Gets or sets the deterministic document identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stable seed key from buildings.seed.json.
    /// </summary>
    public string SeedKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the created home aggregate ID.
    /// </summary>
    public string UnitId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when this mapping was last written.
    /// </summary>
    public DateTimeOffset SeededAt { get; set; }

    /// <summary>
    /// Creates a deterministic document ID for a home seed key.
    /// </summary>
    public static string BuildId(string seedKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seedKey);
        return $"home-seed:{seedKey}";
    }
}
