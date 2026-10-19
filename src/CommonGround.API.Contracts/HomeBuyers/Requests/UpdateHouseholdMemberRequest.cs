namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>Request to replace a household member's contact details.</summary>
public sealed record UpdateHouseholdMemberRequest(
    string Name,
    string Email,
    string Phone);
