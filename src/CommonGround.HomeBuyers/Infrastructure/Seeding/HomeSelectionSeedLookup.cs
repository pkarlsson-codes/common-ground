namespace CommonGround.HomeBuyers.Infrastructure.Seeding;

/// <summary>Deterministic lookup from home selection seed keys to created aggregate IDs.</summary>
public sealed class HomeSelectionSeedLookup
{
    /// <summary>Gets or sets the deterministic document identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the stable seed key from home-selections.seed.json.</summary>
    public string SeedKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the created home selection aggregate ID.</summary>
    public string HomeSelectionId { get; set; } = string.Empty;

    /// <summary>Gets or sets the application the selection chose, if it chose one.</summary>
    public string? SelectedApplicationId { get; set; }

    /// <summary>Gets or sets when this mapping was last written.</summary>
    public DateTimeOffset SeededAt { get; set; }

    /// <summary>Creates a deterministic document ID for a home selection seed key.</summary>
    public static string BuildId(string seedKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seedKey);
        return $"home-selection-seed:{seedKey}";
    }
}
