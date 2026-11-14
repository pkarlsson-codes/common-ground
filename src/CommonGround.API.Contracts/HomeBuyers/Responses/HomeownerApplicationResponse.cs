namespace CommonGround.API.Contracts.HomeBuyers.Responses;

/// <summary>
/// Detail response for a homeowner application. Household size, income and accessibility are the
/// snapshot taken at submission; current household state is served by <see cref="HouseholdResponse"/>.
/// <see cref="RemovalReason"/> (NoLongerEligible, NoResponse, FalseInformation or Other), <see cref="RemovalNote"/> and
/// <see cref="RemovedDate"/> are set when the application was removed from the waiting list.
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
    string? RemovalReason,
    string? RemovalNote,
    DateOnly? RemovedDate,
    DateOnly? SelectedDate,
    string? SelectedHomeId,
    string? SelectedLandParcelId,
    string? SelectedBy,
    IReadOnlyList<PassOverDto> PassOvers);
