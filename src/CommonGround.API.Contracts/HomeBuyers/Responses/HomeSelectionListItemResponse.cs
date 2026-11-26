namespace CommonGround.API.Contracts.HomeBuyers.Responses;

/// <summary>A household chosen for a home, in the list across homes. <see cref="SelectionMethod"/> is "Priority" or "Lottery".</summary>
public sealed record HomeSelectionListItemResponse(
    string Id,
    string HomeId,
    string? HomeLabel,
    DateOnly SelectedDate,
    DateTimeOffset RecordedAt,
    string SelectionMethod,
    int ApplicationCount,
    string SelectedApplicationId,
    HouseholdContactDto? ChosenContact);
