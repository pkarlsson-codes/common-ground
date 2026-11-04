namespace CommonGround.Homeownership.Infrastructure.Seeding;

/// <summary>Deterministic lookup from purchase workflow seed keys to the seeded workflow's ID.</summary>
public sealed class PurchaseWorkflowSeedLookup
{
    /// <summary>Gets or sets the deterministic document identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the stable seed key from purchase-workflows.seed.json.</summary>
    public string SeedKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the purchase workflow the seed recorded its steps on.</summary>
    public string PurchaseWorkflowId { get; set; } = string.Empty;

    /// <summary>Gets or sets when this mapping was last written.</summary>
    public DateTimeOffset SeededAt { get; set; }

    /// <summary>Creates a deterministic document ID for a purchase workflow seed key.</summary>
    public static string BuildId(string seedKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seedKey);
        return $"purchase-workflow-seed:{seedKey}";
    }
}
