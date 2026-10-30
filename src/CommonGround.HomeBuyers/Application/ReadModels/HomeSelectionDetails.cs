namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>
/// A home selection with the home's facts and each candidate's priority facts side by side,
/// for staff to compare. Candidates are not scored or ranked.
/// </summary>
public sealed record HomeSelectionDetails(
    HomeSelectionView Selection,
    HomeFactsView? Home,
    IReadOnlyList<HomeSelectionCandidateDetails> Candidates);

/// <summary>
/// A candidate's frozen priority facts, together with its household's current primary contact and
/// whether its application can still be selected today. A candidate stops being selectable once its
/// application leaves the waiting list or its household's readiness lapses.
/// </summary>
public sealed record HomeSelectionCandidateDetails(
    HomeSelectionCandidateEntry Facts,
    HouseholdContact? PrimaryContact,
    string ApplicationPhase,
    bool CanBeSelected);
