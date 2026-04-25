namespace CommonGround.API.Contracts.PropertyRegistry.Responses;

/// <summary>Response DTO for a building assessment list item.</summary>
public sealed record BuildingAssessmentItemResponse(
    string Id,
    string BuildingId,
    decimal AssessedValue,
    DateOnly AssessmentDate,
    string? ConditionNotes,
    string AssessedBy,
    DateTimeOffset RecordedAt);
