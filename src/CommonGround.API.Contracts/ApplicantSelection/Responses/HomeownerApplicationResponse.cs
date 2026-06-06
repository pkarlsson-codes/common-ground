namespace CommonGround.API.Contracts.ApplicantSelection.Responses;

/// <summary>Detail response for a homeowner application.</summary>
public sealed record HomeownerApplicationResponse(
    string Id,
    string ApplicantFullName,
    string ApplicantEmail,
    string? ApplicantPhone,
    int HouseholdSize,
    decimal AnnualIncome,
    bool AccessibilityRequired,
    string Phase,
    string SubmittedAt,
    string SubmittedBy,
    string? EligibilityApprovedDate,
    bool IsHousingCounselingCompleted,
    bool IsCltOrientationCompleted,
    MortgagePrequalificationDto? MortgagePrequalification,
    string? DenialReason,
    string? WithdrawalReason,
    string? AddedToWaitingListDate,
    string? SelectedDate,
    string? SelectedHomeId,
    string? SelectedLandParcelId,
    string? SelectedBy);

/// <summary>Mortgage prequalification details.</summary>
/// <param name="LenderName">Name of the lender.</param>
/// <param name="ApprovedAmount">Approved amount.</param>
/// <param name="ExpiryDate">Prequalification expiry date.</param>
public sealed record MortgagePrequalificationDto(
    string LenderName,
    decimal ApprovedAmount,
    string ExpiryDate);