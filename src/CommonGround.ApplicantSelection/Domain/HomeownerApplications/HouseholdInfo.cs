namespace CommonGround.ApplicantSelection.Domain.HomeownerApplications;

/// <summary>Household characteristics used for eligibility assessment and home matching.</summary>
public sealed record HouseholdInfo(
    int HouseholdSize,
    decimal AnnualIncome,
    bool AccessibilityRequired);