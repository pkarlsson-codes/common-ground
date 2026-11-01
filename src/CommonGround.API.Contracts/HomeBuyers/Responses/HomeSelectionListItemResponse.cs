namespace CommonGround.API.Contracts.HomeBuyers.Responses;

/// <summary>A home selection in the list across homes.</summary>
public sealed record HomeSelectionListItemResponse(
    string Id,
    string HomeId,
    string? HomeLabel,
    string Status,
    DateTimeOffset StartedAt,
    int CandidateCount,
    string? SelectedApplicationId,
    HouseholdContactDto? ChosenContact,
    DateTimeOffset? ClosedAt,
    DateOnly? FellThroughDate);
