namespace CommonGround.API.Contracts.HomeBuyers.Responses;

/// <summary>An approved application on the waiting list, with its household's readiness as of today.</summary>
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
    bool NeedsReverification);
