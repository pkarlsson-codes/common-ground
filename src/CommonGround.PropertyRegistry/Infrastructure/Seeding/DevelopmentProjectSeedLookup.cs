namespace CommonGround.PropertyRegistry.Infrastructure.Seeding;

/// <summary>
/// Deterministic lookup from development project seed keys to created aggregate IDs.
/// </summary>
public sealed class DevelopmentProjectSeedLookup
{
    /// <summary>
    /// Gets or sets the deterministic document identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stable seed key from development-projects.seed.json.
    /// </summary>
    public string SeedKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the created development project aggregate ID.
    /// </summary>
    public string DevelopmentProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when this mapping was last written.
    /// </summary>
    public DateTimeOffset SeededAt { get; set; }

    /// <summary>
    /// Creates a deterministic document ID for a development project seed key.
    /// </summary>
    public static string BuildId(string seedKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seedKey);
        return $"development-project-seed:{seedKey}";
    }
}
