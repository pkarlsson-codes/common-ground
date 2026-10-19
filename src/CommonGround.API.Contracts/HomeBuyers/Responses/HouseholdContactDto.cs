namespace CommonGround.API.Contracts.HomeBuyers.Responses;

/// <summary>Contact details for a household's primary member.</summary>
public sealed record HouseholdContactDto(
    string Name,
    string Email,
    string Phone);
