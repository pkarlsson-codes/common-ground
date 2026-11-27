namespace CommonGround.API.Contracts.HomeBuyers.Responses;

/// <summary>
/// Detail response for a homeowner application. Household size, income and accessibility follow the household
/// while the application is submitted or approved; a selected application keeps the figures it was chosen on.
/// <see cref="QualificationToConfirmSince"/> is set while an approved household's income or size has changed since
/// staff last checked it, with <see cref="QualifiedAnnualIncome"/> and <see cref="QualifiedHouseholdSize"/> the figures
/// they checked. <see cref="QualificationConfirmedDate"/> and <see cref="QualificationConfirmedBy"/> record the last
/// confirmation.
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
    IReadOnlyList<PassOverDto> PassOvers,
    DateOnly? QualificationToConfirmSince,
    decimal? QualifiedAnnualIncome,
    int? QualifiedHouseholdSize,
    DateOnly? QualificationConfirmedDate,
    string? QualificationConfirmedBy);
