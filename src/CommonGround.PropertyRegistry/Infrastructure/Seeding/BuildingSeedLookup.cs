namespace CommonGround.PropertyRegistry.Infrastructure.Seeding;

/// <summary>
/// Deterministic lookup from building seed keys to created aggregate IDs.
/// </summary>
public sealed class BuildingSeedLookup
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
    /// Gets or sets the created building aggregate ID.
    /// </summary>
    public string BuildingId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when this mapping was last written.
    /// </summary>
    public DateTimeOffset SeededAt { get; set; }

    /// <summary>
    /// Creates a deterministic document ID for a building seed key.
    /// </summary>
    public static string BuildId(string seedKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seedKey);
        return $"building-seed:{seedKey}";
    }
}
