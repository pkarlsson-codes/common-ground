namespace CommonGround.API.Contracts.HomeBuyers.Responses;

/// <summary>
/// Detail response for a homeowner application. Household size, income and accessibility are the
/// snapshot taken at submission; current household state is served by <see cref="HouseholdResponse"/>.
/// <see cref="PassOvers"/> covers all of the household's applications, oldest first.
/// </summary>
public sealed record HomeownerApplicationResponse(
    string Id,
    string HouseholdId,
    HouseholdContactDto? PrimaryContact,
    int HouseholdSize,
    decimal AnnualIncome,
    bool AccessibilityRequired,
    string Phase,
    DateTimeOffset SubmittedAt,
    string SubmittedBy,
    DateOnly? ApprovedDate,
    string? DenialReason,
    string? WithdrawalReason,
    DateOnly? SelectedDate,
    string? SelectedHomeId,
    string? SelectedLandParcelId,
    string? SelectedBy,
    IReadOnlyList<PassOverDto> PassOvers);
