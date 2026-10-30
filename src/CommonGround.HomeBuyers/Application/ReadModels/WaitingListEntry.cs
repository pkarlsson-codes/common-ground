namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>
/// An approved application on the waiting list, together with its household's current readiness.
/// <see cref="IsReadyForSelection"/> and <see cref="NeedsReverification"/> are evaluated against the
/// query date, so they are never stale. <see cref="PassOverCount"/> counts the home selections the
/// household was passed over in, across all its applications.
/// </summary>
public sealed record WaitingListEntry(
    HomeownerApplicationListItemView Application,
    HouseholdListItemView? Household,
    bool IsReadyForSelection,
    bool NeedsReverification,
    int PassOverCount);
