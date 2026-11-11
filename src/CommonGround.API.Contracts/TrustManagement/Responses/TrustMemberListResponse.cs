namespace CommonGround.API.Contracts.TrustManagement.Responses;

/// <summary>The users who belong to a trust.</summary>
/// <param name="Items">The members, by name.</param>
public sealed record TrustMemberListResponse(IReadOnlyList<TrustMemberResponse> Items);

/// <summary>A trust member.</summary>
/// <param name="UserId">The user's id.</param>
/// <param name="Name">The user's name, or null when the sign-in server gave none.</param>
/// <param name="Email">The user's email address, or null when the sign-in server gave none.</param>
public sealed record TrustMemberResponse(string UserId, string? Name, string? Email);
