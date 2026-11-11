namespace CommonGround.API.Contracts.TrustManagement.Requests;

/// <summary>An invite link's token, sent in the body so it stays out of URLs and request logs.</summary>
/// <param name="Token">The token from the invite link.</param>
public sealed record TrustInvitationTokenRequest(string Token);
