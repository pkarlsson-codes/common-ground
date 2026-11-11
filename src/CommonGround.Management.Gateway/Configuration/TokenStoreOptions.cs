namespace CommonGround.Management.Gateway.Configuration;

/// <summary>
/// Options for the Postgres session store, from <c>Authentication:TokenStore</c>.
/// </summary>
public sealed class TokenStoreOptions
{
    /// <summary>Gets or sets how long a session is kept after it is saved. Defaults to 7 days.</summary>
    public int DefaultTtlMinutes { get; set; } = 60 * 24 * 7;

    /// <summary>Gets or sets how often expired sessions are deleted. Defaults to 60 minutes.</summary>
    public int CleanupIntervalMinutes { get; set; } = 60;
}
