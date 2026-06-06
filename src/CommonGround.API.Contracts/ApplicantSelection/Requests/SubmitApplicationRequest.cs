namespace CommonGround.API.Contracts.ApplicantSelection.Requests;

/// <summary>Request to submit a new homeowner application.</summary>
public sealed record SubmitApplicationRequest(
    string ApplicantFullName,
    string ApplicantEmail,
    string? ApplicantPhone,
    int HouseholdSize,
    decimal AnnualIncome,
    bool AccessibilityRequired);