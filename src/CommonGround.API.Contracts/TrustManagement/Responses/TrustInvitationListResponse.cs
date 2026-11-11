namespace CommonGround.API.Contracts.TrustManagement.Responses;

/// <summary>A trust's invitations that can still be used.</summary>
/// <param name="Items">The invitations, newest first.</param>
public sealed record TrustInvitationListResponse(IReadOnlyList<TrustInvitationResponse> Items);

/// <summary>A pending invitation.</summary>
/// <param name="Id">The invitation id.</param>
/// <param name="Email">The address it was made for.</param>
/// <param name="InvitedByName">The name of the member who made it, or null when unknown.</param>
/// <param name="IssuedAt">When it was made.</param>
/// <param name="ExpiresAt">When the link stops working.</param>
public sealed record TrustInvitationResponse(string Id, string Email, string? InvitedByName, DateTimeOffset IssuedAt, DateTimeOffset ExpiresAt);
