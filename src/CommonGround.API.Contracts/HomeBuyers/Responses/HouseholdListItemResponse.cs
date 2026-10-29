namespace CommonGround.API.Contracts.HomeBuyers.Responses;

/// <summary>Summary item for household list responses.</summary>
public sealed record HouseholdListItemResponse(
    string Id,
    HouseholdContactDto PrimaryContact,
    int HouseholdSize,
    bool AccessibilityRequired,
    bool IsHousingCounselingCompleted,
    bool IsCltOrientationCompleted,
    DateOnly? MortgagePrequalificationExpiryDate,
    string RegisteredAt);
