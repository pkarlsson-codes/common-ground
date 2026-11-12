namespace CommonGround.API.Contracts.HomeBuyers.Responses;

/// <summary>
/// A home selection with the home's facts and each candidate's priority facts side by side, in
/// waiting-list order. <see cref="Status"/> is "Open", "Completed" or "Cancelled";
/// <see cref="SelectionMethod"/> is "Priority" or "Lottery" once completed.
/// <see cref="IncomeLimitPercentOfAmi"/> is the home's income limit the candidates were judged against
/// when the selection started, null when it had none, and <see cref="IncomeLimitsEffectiveFrom"/> the
/// effective date of the income limits used.
/// </summary>
public sealed record HomeSelectionResponse(
    string Id,
    string HomeId,
    string? HomeLabel,
    int? HomeBedrooms,
    bool? HomeIsAccessible,
    int? IncomeLimitPercentOfAmi,
    DateOnly? IncomeLimitsEffectiveFrom,
    string Status,
    string StartedBy,
    DateTimeOffset StartedAt,
    IReadOnlyList<HomeSelectionCandidateDto> Candidates,
    string? SelectedApplicationId,
    string? SelectionMethod,
    IReadOnlyList<string> Priorities,
    string? SelectionReason,
    int? LotterySeed,
    IReadOnlyList<string> LotteryPoolApplicationIds,
    IReadOnlyList<string> LotteryIneligibleApplicationIds,
    string? CancellationReason,
    string? ClosedBy,
    DateTimeOffset? ClosedAt,
    string? FellThroughReason,
    DateOnly? FellThroughDate);

/// <summary>
/// A candidate's priority facts as they stood when the selection started, with its household's
/// current primary contact and whether its application can still be selected today.
/// <see cref="IncomeLimit"/> is the home's limit for the household's size, null when the home has none;
/// a candidate with <see cref="IsOverIncomeLimit"/> can't be selected.
/// </summary>
public sealed record HomeSelectionCandidateDto(
    string ApplicationId,
    string HouseholdId,
    HouseholdContactDto? PrimaryContact,
    decimal AnnualIncome,
    int HouseholdSize,
    bool RequiresAccessibility,
    DateOnly WaitingSince,
    decimal? IncomeLimit,
    bool IsOverIncomeLimit,
    string ApplicationPhase,
    bool CanBeSelected);
