namespace CommonGround.Identity.Infrastructure.Seeding;

/// <summary>Deterministic lookup from user seed keys to created aggregate IDs.</summary>
public sealed class UserSeedLookup
{
    /// <summary>Gets or sets the deterministic document identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the stable seed key from users.seed.json.</summary>
    public string SeedKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the created user aggregate ID.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Gets or sets when this mapping was last written.</summary>
    public DateTimeOffset SeededAt { get; set; }

    /// <summary>Creates a deterministic document ID for a user seed key.</summary>
    public static string BuildId(string seedKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seedKey);
        return $"user-seed:{seedKey}";
    }
}
