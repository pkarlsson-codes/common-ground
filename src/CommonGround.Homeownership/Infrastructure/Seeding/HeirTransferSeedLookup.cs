namespace CommonGround.Homeownership.Infrastructure.Seeding;

/// <summary>Deterministic lookup from heir transfer seed keys to the transfer the seed opened.</summary>
public sealed class HeirTransferSeedLookup
{
    /// <summary>Gets or sets the deterministic document identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the stable seed key from heir-transfers.seed.json.</summary>
    public string SeedKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the transfer the seed opened.</summary>
    public string HeirTransferId { get; set; } = string.Empty;

    /// <summary>Gets or sets when this mapping was last written.</summary>
    public DateTimeOffset SeededAt { get; set; }

    /// <summary>Creates a deterministic document ID for a heir transfer seed key.</summary>
    public static string BuildId(string seedKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seedKey);
        return $"heir-transfer-seed:{seedKey}";
    }
}
