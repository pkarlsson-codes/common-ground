namespace CommonGround.API.Contracts.HomeBuyers.Responses;

/// <summary>
/// A home selection with the home's facts and each candidate's priority facts side by side, in
/// waiting-list order. <see cref="Status"/> is "Open", "Completed" or "Cancelled";
/// <see cref="SelectionMethod"/> is "Priority" or "Lottery" once completed.
/// </summary>
public sealed record HomeSelectionResponse(
    string Id,
    string HomeId,
    string? HomeLabel,
    int? HomeBedrooms,
    bool? HomeIsAccessible,
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
/// </summary>
public sealed record HomeSelectionCandidateDto(
    string ApplicationId,
    string HouseholdId,
    HouseholdContactDto? PrimaryContact,
    decimal AnnualIncome,
    int HouseholdSize,
    bool RequiresAccessibility,
    DateOnly WaitingSince,
    string ApplicationPhase,
    bool CanBeSelected);
