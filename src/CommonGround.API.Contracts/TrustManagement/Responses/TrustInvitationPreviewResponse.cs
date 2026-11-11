namespace CommonGround.API.Contracts.TrustManagement.Responses;

/// <summary>What the invite page shows before the user joins.</summary>
/// <param name="TrustName">The trust the link joins.</param>
/// <param name="Email">The address the invitation was made for.</param>
public sealed record TrustInvitationPreviewResponse(string TrustName, string Email);
