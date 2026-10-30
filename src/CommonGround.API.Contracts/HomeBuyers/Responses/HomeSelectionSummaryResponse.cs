namespace CommonGround.API.Contracts.HomeBuyers.Responses;

/// <summary>
/// A selection started for a home. <see cref="Status"/> is "Open", "Completed" or "Cancelled";
/// <see cref="SelectionMethod"/> is "Priority" or "Lottery" once completed.
/// </summary>
public sealed record HomeSelectionSummaryResponse(
    string Id,
    string HomeId,
    string Status,
    string StartedBy,
    DateTimeOffset StartedAt,
    int CandidateCount,
    string? SelectedApplicationId,
    string? SelectionMethod,
    DateTimeOffset? ClosedAt);
