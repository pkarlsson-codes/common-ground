namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>Request to add a non-primary member to a household.</summary>
public sealed record AddHouseholdMemberRequest(
    string Name,
    string Email,
    string Phone);
