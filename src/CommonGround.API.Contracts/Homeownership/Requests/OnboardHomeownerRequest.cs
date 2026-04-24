namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to onboard the first homeowner to a ground lease.</summary>
public sealed record OnboardHomeownerRequest(
    string FullName,
    string Email,
    string? Phone);
