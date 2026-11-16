namespace CommonGround.TrustManagement.Application.ReadModels;

/// <summary>An invitation to join a trust, for finding it by its link's token hash and listing a trust's pending ones.</summary>
public sealed class TrustInvitationView
{
    /// <summary>Status of an invitation nobody has used or cancelled. It may still have expired.</summary>
    public const string Pending = "Pending";

    /// <summary>Status of an invitation someone joined with.</summary>
    public const string Accepted = "Accepted";

    /// <summary>Status of an invitation a member cancelled.</summary>
    public const string Cancelled = "Cancelled";

    public string Id { get; set; } = string.Empty;

    public string TrustId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the SHA-256 hash of the link's token.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public string IssuedBy { get; set; } = string.Empty;

    public DateTimeOffset IssuedAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>Gets or sets <see cref="Pending"/>, <see cref="Accepted"/> or <see cref="Cancelled"/>.</summary>
    public string Status { get; set; } = Pending;

    /// <summary>Gets or sets the user who joined with it, once accepted.</summary>
    public string? AcceptedBy { get; set; }
}
