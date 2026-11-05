namespace CommonGround.Stewardship.Infrastructure.Seeding;

/// <summary>Deterministic lookup from foreclosure prevention case seed keys to the case the seed opened.</summary>
public sealed class ForeclosurePreventionCaseSeedLookup
{
    /// <summary>Gets or sets the deterministic document identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the stable seed key from foreclosure-prevention-cases.seed.json.</summary>
    public string SeedKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the foreclosure prevention case the seed opened.</summary>
    public string ForeclosurePreventionCaseId { get; set; } = string.Empty;

    /// <summary>Gets or sets when this mapping was last written.</summary>
    public DateTimeOffset SeededAt { get; set; }

    /// <summary>Creates a deterministic document ID for a foreclosure prevention case seed key.</summary>
    public static string BuildId(string seedKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seedKey);
        return $"foreclosure-prevention-case-seed:{seedKey}";
    }
}
