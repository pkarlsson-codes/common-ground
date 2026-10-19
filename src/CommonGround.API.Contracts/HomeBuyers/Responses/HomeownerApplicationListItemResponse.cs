namespace CommonGround.API.Contracts.HomeBuyers.Responses;

/// <summary>Summary item for homeowner application list responses.</summary>
public sealed record HomeownerApplicationListItemResponse(
    string Id,
    string HouseholdId,
    HouseholdContactDto? PrimaryContact,
    int HouseholdSize,
    bool AccessibilityRequired,
    string Phase,
    string SubmittedAt);
