namespace CommonGround.Homeowner.Gateway.Models;

/// <summary>
/// DTO representing the authenticated user details.
/// </summary>
public sealed class CurrentUserDto
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Display username for the user.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// Email address of the user.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Trust identifier for the user when membership exists.
    /// </summary>
    public string? TrustId { get; set; }

    /// <summary>
    /// Indicates whether the user currently belongs to a trust.
    /// </summary>
    public bool HasTrust { get; set; }
}
