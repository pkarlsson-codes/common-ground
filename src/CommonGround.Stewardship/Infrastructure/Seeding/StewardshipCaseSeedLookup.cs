namespace CommonGround.Stewardship.Infrastructure.Seeding;

/// <summary>Deterministic lookup from stewardship case seed keys to the case the seed filled in.</summary>
public sealed class StewardshipCaseSeedLookup
{
    /// <summary>Gets or sets the deterministic document identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the stable seed key from stewardship-cases.seed.json.</summary>
    public string SeedKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the stewardship case the seed filled in.</summary>
    public string StewardshipCaseId { get; set; } = string.Empty;

    /// <summary>Gets or sets when this mapping was last written.</summary>
    public DateTimeOffset SeededAt { get; set; }

    /// <summary>Creates a deterministic document ID for a stewardship case seed key.</summary>
    public static string BuildId(string seedKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seedKey);
        return $"stewardship-case-seed:{seedKey}";
    }
}
