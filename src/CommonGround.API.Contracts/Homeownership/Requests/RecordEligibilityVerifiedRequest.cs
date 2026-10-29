namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record buyer eligibility verified.</summary>
public sealed record RecordEligibilityVerifiedRequest(
    DateOnly VerifiedDate,
    string? Notes);
