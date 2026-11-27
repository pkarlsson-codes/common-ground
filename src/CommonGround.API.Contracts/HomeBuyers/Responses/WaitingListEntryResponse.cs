namespace CommonGround.API.Contracts.HomeBuyers.Responses;

/// <summary>
/// An approved application on the waiting list, with its household's readiness as of today and the
/// number of home selections the household was passed over in. <see cref="QualificationToConfirmSince"/> is set while
/// the household's income or size has changed since staff last checked it still qualifies.
/// </summary>
public sealed record WaitingListEntryResponse(
    string ApplicationId,
    string HouseholdId,
    HouseholdContactDto? PrimaryContact,
    int HouseholdSize,
    bool AccessibilityRequired,
    DateOnly? ApprovedDate,
    bool IsHousingCounselingCompleted,
    bool IsCltOrientationCompleted,
    DateOnly? MortgagePrequalificationExpiryDate,
    bool IsReadyForSelection,
    bool NeedsReverification,
    int PassOverCount,
    DateOnly? QualificationToConfirmSince);
