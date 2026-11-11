namespace CommonGround.API.Contracts.TrustManagement.Responses;

/// <summary>The trust a user joined with an invite link.</summary>
/// <param name="TrustId">The trust's id.</param>
public sealed record AcceptTrustInvitationResponse(string TrustId);
