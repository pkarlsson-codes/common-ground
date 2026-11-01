namespace CommonGround.API.Contracts.HomeBuyers.Responses;

/// <summary>Detail response for a household's current state.</summary>
public sealed record HouseholdResponse(
    string Id,
    IReadOnlyList<HouseholdMemberDto> Members,
    int HouseholdSize,
    decimal AnnualIncome,
    bool AccessibilityRequired,
    bool IsHousingCounselingCompleted,
    DateOnly? HousingCounselingCompletedDate,
    string? HousingCounselingCertificationNumber,
    string? HousingCounselorOrganization,
    bool IsCltOrientationCompleted,
    DateOnly? CltOrientationCompletedDate,
    MortgagePrequalificationDto? MortgagePrequalification,
    DateTimeOffset RegisteredAt,
    string RegisteredBy,
    IReadOnlyList<HouseholdApplicationDto> Applications,
    HouseholdJourneyDto Journey);

/// <summary>
/// Where the household is on its way to owning a home: Stage is Intake, Ready, Waiting, Purchasing or Homeowner,
/// with the date each step so far was reached.
/// </summary>
public sealed record HouseholdJourneyDto(
    string Stage,
    DateOnly? ReadyDate,
    DateOnly? ApprovedDate,
    DateOnly? SelectedDate,
    DateOnly? PurchasedDate,
    string? CurrentApplicationId,
    string? SelectedHomeId);

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
    DateOnly ExpiryDate);

/// <summary>An application submitted by the household.</summary>
/// <param name="Id">Application identifier.</param>
/// <param name="Phase">Current application phase.</param>
/// <param name="SubmittedAt">When the application was submitted.</param>
public sealed record HouseholdApplicationDto(
    string Id,
    string Phase,
    DateTimeOffset SubmittedAt);
