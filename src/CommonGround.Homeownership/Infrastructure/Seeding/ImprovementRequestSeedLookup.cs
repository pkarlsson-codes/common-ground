namespace CommonGround.Homeownership.Infrastructure.Seeding;

/// <summary>Deterministic lookup from improvement request seed keys to the request the seed recorded.</summary>
public sealed class ImprovementRequestSeedLookup
{
    /// <summary>Gets or sets the deterministic document identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the stable seed key from improvement-requests.seed.json.</summary>
    public string SeedKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the request the seed recorded.</summary>
    public string ImprovementRequestId { get; set; } = string.Empty;

    /// <summary>Gets or sets when this mapping was last written.</summary>
    public DateTimeOffset SeededAt { get; set; }

    /// <summary>Creates a deterministic document ID for an improvement request seed key.</summary>
    public static string BuildId(string seedKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seedKey);
        return $"improvement-request-seed:{seedKey}";
    }
}
