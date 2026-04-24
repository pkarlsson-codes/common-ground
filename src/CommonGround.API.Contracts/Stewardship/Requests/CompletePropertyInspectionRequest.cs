namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Request to record the completion of a property inspection.</summary>
public sealed record CompletePropertyInspectionRequest(
    bool OccupancyConfirmed,
    string? ConditionNotes,
    DateOnly CompletedDate);
