namespace CommonGround.SharedKernel.Infrastructure.Seeding;

/// <summary>
/// One run of the seeders. Each run seeds its own trusts from the same seed files, so a run can fill a
/// new trust beside the ones earlier runs made.
/// </summary>
/// <remarks>
/// Only the trust lookup needs the run key: every other seed lookup is a document in its trust's tenant,
/// so a new trust starts with none.
/// </remarks>
public sealed record SeedRun
{
    /// <summary>The run <c>db:seed</c> uses. Its trust lookups keep the IDs they had before runs existed.</summary>
    public static readonly SeedRun Development = new("dev");

    /// <summary>Initializes a new instance of the <see cref="SeedRun"/> class.</summary>
    /// <param name="key">The run's key. Trust seed lookups are namespaced by it.</param>
    /// <exception cref="ArgumentException"><paramref name="key"/> is blank or contains ':'.</exception>
    public SeedRun(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Contains(':', StringComparison.Ordinal))
        {
            throw new ArgumentException("A seed run key can't be blank or contain ':'.", nameof(key));
        }

        Key = key;
    }

    /// <summary>Gets the run's key.</summary>
    public string Key { get; }

    /// <summary>Gets whether this is the <see cref="Development"/> run.</summary>
    public bool IsDevelopment => this == Development;
}
