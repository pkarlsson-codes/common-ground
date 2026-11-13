using CommonGround.SharedKernel.Domain;

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

    /// <summary>
    /// Gets the ID and name the run's trust gets instead of the seed file's, or <see langword="null"/> to use the
    /// seed file's. Only for a seed file with one trust.
    /// </summary>
    public SeedRunTrust? Trust { get; init; }

    /// <summary>Gets whether this is the <see cref="Development"/> run.</summary>
    public bool IsDevelopment => this == Development;
}

/// <summary>The ID and name a seed run gives its trust instead of the seed file's.</summary>
public sealed record SeedRunTrust
{
    /// <summary>Initializes a new instance of the <see cref="SeedRunTrust"/> class.</summary>
    /// <param name="id">The trust ID.</param>
    /// <param name="name">The trust name.</param>
    public SeedRunTrust(TrustId id, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Id = id;
        Name = name;
    }

    /// <summary>Gets the trust ID.</summary>
    public TrustId Id { get; }

    /// <summary>Gets the trust name.</summary>
    public string Name { get; }
}
