namespace CommonGround.Identity.Application.ReadModels;

/// <summary>Read model for user lookup by Keycloak subject. Supports fast identity resolution without event sourcing.</summary>
public sealed class UserView
{
    /// <summary>Gets the Keycloak subject (primary key for this read model).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets the CommonGround UserId.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Gets the trust this user belongs to, if assigned.</summary>
    public string? TrustId { get; set; }
}
