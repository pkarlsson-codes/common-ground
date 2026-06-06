namespace CommonGround.API.Contracts.ApplicantSelection.Requests;

/// <summary>Request to deny eligibility.</summary>
public sealed record DenyEligibilityRequest(
    string Reason,
    string DeniedDate);