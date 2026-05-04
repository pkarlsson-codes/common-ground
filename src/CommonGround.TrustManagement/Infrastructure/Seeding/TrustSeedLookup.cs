namespace CommonGround.TrustManagement.Infrastructure.Seeding;

/// <summary>Deterministic lookup from trust seed keys to created aggregate IDs.</summary>
public sealed class TrustSeedLookup
{
    /// <summary>Gets or sets the deterministic document identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the stable seed key from trusts.seed.json.</summary>
    public string SeedKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the created trust aggregate ID.</summary>
    public string TrustId { get; set; } = string.Empty;

    /// <summary>Gets or sets when this mapping was last written.</summary>
    public DateTimeOffset SeededAt { get; set; }

    /// <summary>Creates a deterministic document ID for a trust seed key.</summary>
    public static string BuildId(string seedKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seedKey);
        return $"trust-seed:{seedKey}";
    }
}
