namespace CommonGround.API.Contracts.TrustManagement.Requests;

/// <summary>Makes an invite link to the caller's trust.</summary>
/// <param name="Email">The colleague's email address.</param>
public sealed record CreateTrustInvitationRequest(string Email);
