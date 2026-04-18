namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>Request to record a new building assessment.</summary>
public sealed record RecordBuildingAssessmentRequest(
    decimal AssessedValue,
    DateOnly AssessmentDate,
    string? ConditionNotes);