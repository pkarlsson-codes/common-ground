namespace CommonGround.Reminders.Infrastructure.Seeding;

/// <summary>Deterministic lookup from reminder seed keys to the reminder the seed set.</summary>
public sealed class ReminderSeedLookup
{
    /// <summary>Gets or sets the deterministic document identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the stable seed key from reminders.seed.json.</summary>
    public string SeedKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the reminder the seed set.</summary>
    public string ReminderId { get; set; } = string.Empty;

    /// <summary>Gets or sets when this mapping was last written.</summary>
    public DateTimeOffset SeededAt { get; set; }

    /// <summary>Creates a deterministic document ID for a reminder seed key.</summary>
    /// <param name="seedKey">The seed key.</param>
    /// <returns>The document ID.</returns>
    public static string BuildId(string seedKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seedKey);
        return $"reminder-seed:{seedKey}";
    }
}
