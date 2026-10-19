namespace CommonGround.API.Contracts.HomeBuyers.Responses;

/// <summary>
/// Detail response for a homeowner application. Household size, income and accessibility are the
/// snapshot taken at submission; current household state is served by <see cref="HouseholdResponse"/>.
/// </summary>
public sealed record HomeownerApplicationResponse(
    string Id,
    string HouseholdId,
    HouseholdContactDto? PrimaryContact,
    int HouseholdSize,
    decimal AnnualIncome,
    bool AccessibilityRequired,
    string Phase,
    string SubmittedAt,
    string SubmittedBy,
    string? ApprovedDate,
    string? DenialReason,
    string? WithdrawalReason,
    string? SelectedDate,
    string? SelectedHomeId,
    string? SelectedLandParcelId,
    string? SelectedBy);
