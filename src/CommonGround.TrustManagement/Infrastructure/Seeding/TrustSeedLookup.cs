using CommonGround.SharedKernel.Infrastructure.Seeding;

namespace CommonGround.TrustManagement.Infrastructure.Seeding;

/// <summary>
/// Lookup from a seed run and trust seed key to the created trust. Stored in the default tenant, so the
/// key includes the run.
/// </summary>
public sealed class TrustSeedLookup
{
    /// <summary>Gets or sets the deterministic document identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the key of the seed run that created the trust.</summary>
    public string RunKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the stable seed key from trusts.seed.json.</summary>
    public string SeedKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the created trust aggregate ID.</summary>
    public string TrustId { get; set; } = string.Empty;

    /// <summary>Gets or sets when this mapping was last written.</summary>
    public DateTimeOffset SeededAt { get; set; }

    /// <summary>
    /// Creates the document ID for a trust seed key in a run. The <see cref="SeedRun.Development"/> run keeps
    /// the IDs it had before runs existed.
    /// </summary>
    /// <param name="run">The seed run.</param>
    /// <param name="seedKey">The trust seed key.</param>
    /// <returns>The document ID.</returns>
    public static string BuildId(SeedRun run, string seedKey)
    {
        ArgumentNullException.ThrowIfNull(run);
        ArgumentException.ThrowIfNullOrWhiteSpace(seedKey);
        return run.IsDevelopment ? $"trust-seed:{seedKey}" : $"trust-seed:{run.Key}:{seedKey}";
    }
}
