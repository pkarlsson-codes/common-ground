namespace CommonGround.Identity.Infrastructure.Persistence;

/// <summary>Marten document used as an O(1) lookup from Keycloak subject to owned UserId and TrustId.</summary>
public sealed class UserSubjectIndex
{
    /// <summary>Gets or sets the Keycloak subject (PK).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the owned UserId value.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the trust the user belongs to.</summary>
    public string TrustId { get; set; } = string.Empty;
}
