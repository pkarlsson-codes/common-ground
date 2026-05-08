namespace CommonGround.API.Contracts.Identity.Responses;

/// <summary>Response returned by the EnsureUser endpoint containing the stable owned user ID and trust membership.</summary>
public sealed record EnsureUserResponse(string UserId, string? TrustId);
