namespace CommonGround.Identity.Application.ReadModels;

/// <summary>Read model for user lookup by Keycloak subject. Supports fast identity resolution without event sourcing.</summary>
public sealed class UserView
{
    /// <summary>Gets or sets the Keycloak subject (primary key for this read model).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the Keycloak subject used for idempotent user lookup.</summary>
    public string KeycloakSubject { get; set; } = string.Empty;

    /// <summary>Gets or sets the CommonGround UserId, as opposed to the Keycloak subject.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the email address the sign-in server gave at registration, if any.</summary>
    public string? Email { get; set; }

    /// <summary>Gets or sets the name the sign-in server gave at registration, if any.</summary>
    public string? DisplayName { get; set; }

    /// <summary>Gets or sets the trust this user belongs to, or null if not assigned.</summary>
    public string? TrustId { get; set; }
}
