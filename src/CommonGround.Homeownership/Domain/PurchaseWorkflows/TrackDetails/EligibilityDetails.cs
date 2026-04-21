namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.TrackDetails;

/// <summary>Details recorded when buyer eligibility is verified.</summary>
/// <param name="VerifiedDate">Date eligibility was verified.</param>
/// <param name="Notes">Optional notes about the verification.</param>
public sealed record EligibilityDetails(DateOnly VerifiedDate, string? Notes);
