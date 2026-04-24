namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to change the homeowner on a ground lease.</summary>
public sealed record ChangeHomeownerRequest(
    string FullName,
    string Email,
    string? Phone);
