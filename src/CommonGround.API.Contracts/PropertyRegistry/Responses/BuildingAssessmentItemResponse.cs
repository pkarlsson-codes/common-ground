namespace CommonGround.API.Contracts.PropertyRegistry.Responses;

/// <summary>Response DTO for a home assessment list item.</summary>
public sealed record HomeAssessmentItemResponse(
    string Id,
    string HomeId,
    decimal AssessedValue,
    DateOnly AssessmentDate,
    string? ConditionNotes,
    string? AppraiserName,
    string RecordedBy,
    DateTimeOffset RecordedAt);
