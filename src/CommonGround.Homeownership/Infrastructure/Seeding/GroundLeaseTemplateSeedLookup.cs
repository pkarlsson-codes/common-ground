namespace CommonGround.Homeownership.Infrastructure.Seeding;

/// <summary>Deterministic lookup from ground lease template seed keys to created aggregate IDs.</summary>
public sealed class GroundLeaseTemplateSeedLookup
{
    /// <summary>Gets or sets the deterministic document identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the stable seed key from ground-lease-templates.seed.json.</summary>
    public string SeedKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the created ground lease template aggregate ID.</summary>
    public string TemplateId { get; set; } = string.Empty;

    /// <summary>Gets or sets when this mapping was last written.</summary>
    public DateTimeOffset SeededAt { get; set; }

    /// <summary>Creates a deterministic document ID for a ground lease template seed key.</summary>
    public static string BuildId(string seedKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seedKey);
        return $"ground-lease-template-seed:{seedKey}";
    }
}
