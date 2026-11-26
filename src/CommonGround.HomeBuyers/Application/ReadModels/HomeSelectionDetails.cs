namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>
/// A home's selection page: whether a household can be chosen for it now, its waiting list compared against it today,
/// and the decisions already recorded for it, newest first.
/// </summary>
/// <param name="Home">The home's facts.</param>
/// <param name="SaleStage">Where the home stands in being sold, counting a choice PropertyRegistry hasn't heard of yet.</param>
/// <param name="IsOffered">Whether a household can be chosen for the home now.</param>
/// <param name="Today">The trust's today, the date the waiting list is compared on.</param>
/// <param name="IncomeLimitsEffectiveFrom">The effective date of the income limit schedule in effect today, or null when none is.</param>
/// <param name="HasIncomeLimitsInEffect">Whether the home has no income limit, or the schedule in effect has its level.</param>
/// <param name="WaitingList">The waiting list as it is, longest waiting first.</param>
/// <param name="Decisions">The households chosen for the home, newest first.</param>
public sealed record HomeSelectionOverview(
    HomeFactsView Home,
    string SaleStage,
    bool IsOffered,
    DateOnly Today,
    DateOnly? IncomeLimitsEffectiveFrom,
    bool HasIncomeLimitsInEffect,
    IReadOnlyList<WaitingListComparisonEntry> WaitingList,
    IReadOnlyList<HomeSelectionDecision> Decisions);

/// <summary>
/// An application on the waiting list as compared against a home today, with its household's primary contact and the
/// approved amount of its mortgage prequalification (null when it has none). Applications are not scored or ranked.
/// </summary>
/// <param name="ApplicationId">The application.</param>
/// <param name="HouseholdId">The household.</param>
/// <param name="AnnualIncome">The household's annual income as declared on the application.</param>
/// <param name="HouseholdSize">The number of people in the household as declared on the application.</param>
/// <param name="RequiresAccessibility">Whether the household needs an accessible home.</param>
/// <param name="WaitingSince">The date the household joined the waiting list.</param>
/// <param name="IncomeLimit">The home's income limit for the household's size, or null when it has none or none is in effect.</param>
/// <param name="Reasons">Why the household can't be chosen today, as <c>CannotBeChosenReason</c> names; empty when it can.</param>
/// <param name="PrimaryContact">The household's primary contact, if its view has arrived.</param>
/// <param name="PrequalifiedAmount">The approved amount of the household's mortgage prequalification, if it has one.</param>
public sealed record WaitingListComparisonEntry(
    string ApplicationId,
    string HouseholdId,
    decimal AnnualIncome,
    int HouseholdSize,
    bool RequiresAccessibility,
    DateOnly WaitingSince,
    decimal? IncomeLimit,
    IReadOnlyList<string> Reasons,
    HouseholdContact? PrimaryContact,
    decimal? PrequalifiedAmount);

/// <summary>A household chosen for a home, with each application on the waiting list then and its household's primary contact now.</summary>
/// <param name="Selection">The selection.</param>
/// <param name="Applications">The waiting list as it was compared, longest waiting first.</param>
public sealed record HomeSelectionDecision(
    HomeSelectionView Selection,
    IReadOnlyList<HomeSelectionDecisionEntry> Applications);

/// <summary>One application in a recorded choice, with its household's primary contact now.</summary>
/// <param name="Entry">The application as recorded.</param>
/// <param name="PrimaryContact">The household's primary contact, if its view has arrived.</param>
public sealed record HomeSelectionDecisionEntry(
    HomeSelectionApplicationEntry Entry,
    HouseholdContact? PrimaryContact);
