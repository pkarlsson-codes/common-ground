namespace CommonGround.API.Contracts.ApplicantSelection.Responses;

/// <summary>Summary item for homeowner application list responses.</summary>
public sealed record HomeownerApplicationListItemResponse(
    string Id,
    string HouseholdId,
    HouseholdContactDto? PrimaryContact,
    int HouseholdSize,
    bool AccessibilityRequired,
    string Phase,
    string SubmittedAt);
