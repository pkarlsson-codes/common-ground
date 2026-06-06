namespace CommonGround.API.Contracts.ApplicantSelection.Requests;

/// <summary>Request to approve eligibility.</summary>
public sealed record ApproveEligibilityRequest(
    string ApprovedDate,
    string? Notes);