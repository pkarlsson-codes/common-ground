namespace CommonGround.Web.Gateway.Models;

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
}

// NOTE: We intentionally return the user DTO directly on 200 and rely on HTTP 401 for unauthorized.
// The envelope type was removed to follow REST semantics: 200 => user DTO, 401 => unauthorized.
