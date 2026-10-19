namespace CommonGround.API.Contracts.HomeBuyers.Responses;

/// <summary>Detail response for a household's current state.</summary>
public sealed record HouseholdResponse(
    string Id,
    IReadOnlyList<HouseholdMemberDto> Members,
    int HouseholdSize,
    decimal AnnualIncome,
    bool AccessibilityRequired,
    bool IsHousingCounselingCompleted,
    string? HousingCounselingCompletedDate,
    string? HousingCounselingCertificationNumber,
    string? HousingCounselorOrganization,
    bool IsCltOrientationCompleted,
    string? CltOrientationCompletedDate,
    MortgagePrequalificationDto? MortgagePrequalification,
    string RegisteredAt,
    string RegisteredBy,
    IReadOnlyList<HouseholdApplicationDto> Applications);

/// <summary>A household member.</summary>
public sealed record HouseholdMemberDto(
    string Id,
    string Name,
    string Email,
    string Phone,
    bool IsPrimary);

/// <summary>Mortgage prequalification details.</summary>
/// <param name="LenderName">Name of the lender.</param>
/// <param name="ApprovedAmount">Approved amount.</param>
/// <param name="ExpiryDate">Prequalification expiry date.</param>
public sealed record MortgagePrequalificationDto(
    string LenderName,
    decimal ApprovedAmount,
    string ExpiryDate);

/// <summary>An application submitted by the household.</summary>
/// <param name="Id">Application identifier.</param>
/// <param name="Phase">Current application phase.</param>
/// <param name="SubmittedAt">When the application was submitted.</param>
public sealed record HouseholdApplicationDto(
    string Id,
    string Phase,
    string SubmittedAt);
