namespace CommonGround.API.Contracts.HomeBuyers.Responses;

/// <summary>
/// A home's selection page: the home's facts, whether a household can be chosen for it now, its waiting list compared
/// against it today in waiting-list order, and the households already chosen for it, newest first.
/// <see cref="SaleStage"/> is "NotForSale", "AwaitingTrustDecision", "Offered", "UnderContract" or "HeldByTrust";
/// a household can be chosen only when <see cref="IsOffered"/>. <see cref="IncomeLimitPercentOfAmi"/> is the home's
/// income limit, null when it has none, and <see cref="IncomeLimitsEffectiveFrom"/> the effective date of the
/// schedule in effect today, null when none is. Choosing is refused when <see cref="HasIncomeLimitsInEffect"/> is false.
/// </summary>
public sealed record HomeSelectionOverviewResponse(
    string HomeId,
    string HomeLabel,
    int HomeBedrooms,
    decimal HomeBathrooms,
    decimal HomeSquareFootage,
    bool HomeIsAccessible,
    int? IncomeLimitPercentOfAmi,
    DateOnly? IncomeLimitsEffectiveFrom,
    bool HasIncomeLimitsInEffect,
    string SaleStage,
    bool IsOffered,
    DateOnly Today,
    IReadOnlyList<WaitingListComparisonEntryDto> WaitingList,
    IReadOnlyList<HomeSelectionDecisionDto> Decisions);

/// <summary>
/// An application on the waiting list compared against the home today. <see cref="Reasons"/> say why the household
/// can't be chosen: "OverIncomeLimit", "NoPrimaryMember", "HousingCounselingIncomplete", "CltOrientationIncomplete",
/// "NoPrequalification" or "PrequalificationExpired"; empty when it can.
/// </summary>
public sealed record WaitingListComparisonEntryDto(
    string ApplicationId,
    string HouseholdId,
    HouseholdContactDto? PrimaryContact,
    decimal AnnualIncome,
    int HouseholdSize,
    bool RequiresAccessibility,
    DateOnly WaitingSince,
    decimal? IncomeLimit,
    bool CanBeChosen,
    IReadOnlyList<string> Reasons,
    decimal? PrequalifiedAmount);

/// <summary>
/// A household chosen for the home. <see cref="SelectionMethod"/> is "Priority" or "Lottery". <see cref="Applications"/>
/// is the whole waiting list at that moment. <see cref="LotteryTiedApplicationIds"/> is null unless a lottery drew from
/// only the households staff found tied.
/// </summary>
public sealed record HomeSelectionDecisionDto(
    string Id,
    string SelectedApplicationId,
    string SelectedHouseholdId,
    DateOnly SelectedDate,
    string SelectedBy,
    DateTimeOffset RecordedAt,
    string SelectionMethod,
    string GroundLeaseTemplateId,
    decimal PurchasePrice,
    int? IncomeLimitPercentOfAmi,
    DateOnly? IncomeLimitsEffectiveFrom,
    IReadOnlyList<string> Priorities,
    string? SelectionReason,
    IReadOnlyList<string>? LotteryTiedApplicationIds,
    IReadOnlyList<string> LotteryPoolApplicationIds,
    int? LotterySeed,
    IReadOnlyList<HomeSelectionDecisionApplicationDto> Applications);

/// <summary>
/// One application on the waiting list when a household was chosen. <see cref="Outcome"/> is "Chosen", "PassedOver" or
/// "CouldNotBeChosen"; <see cref="Reasons"/> say why it couldn't be chosen, as on <see cref="WaitingListComparisonEntryDto"/>.
/// </summary>
public sealed record HomeSelectionDecisionApplicationDto(
    string ApplicationId,
    string HouseholdId,
    HouseholdContactDto? PrimaryContact,
    decimal AnnualIncome,
    int HouseholdSize,
    bool RequiresAccessibility,
    DateOnly WaitingSince,
    decimal? IncomeLimit,
    string Outcome,
    IReadOnlyList<string> Reasons);
