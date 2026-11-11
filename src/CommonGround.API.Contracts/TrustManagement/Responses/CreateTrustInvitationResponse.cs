namespace CommonGround.API.Contracts.TrustManagement.Responses;

/// <summary>A new invitation. The token is returned only here; the link is built from it.</summary>
/// <param name="InvitationId">The invitation id.</param>
/// <param name="Token">The secret token for the invite link.</param>
/// <param name="ExpiresAt">When the link stops working.</param>
public sealed record CreateTrustInvitationResponse(string InvitationId, string Token, DateTimeOffset ExpiresAt);
