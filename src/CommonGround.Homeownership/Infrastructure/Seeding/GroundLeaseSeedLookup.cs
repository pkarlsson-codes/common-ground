namespace CommonGround.Homeownership.Infrastructure.Seeding;

/// <summary>Deterministic lookup from ground lease seed keys to the seeded lease's ID.</summary>
public sealed class GroundLeaseSeedLookup
{
    /// <summary>Gets or sets the deterministic document identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the stable seed key from ground-leases.seed.json.</summary>
    public string SeedKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the ground lease the seed created or found.</summary>
    public string GroundLeaseId { get; set; } = string.Empty;

    /// <summary>Gets or sets when this mapping was last written.</summary>
    public DateTimeOffset SeededAt { get; set; }

    /// <summary>Creates a deterministic document ID for a ground lease seed key.</summary>
    public static string BuildId(string seedKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seedKey);
        return $"ground-lease-seed:{seedKey}";
    }
}
