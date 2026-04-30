namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>Request to record a new home assessment.</summary>
public sealed record RecordHomeAssessmentRequest(
    decimal AssessedValue,
    DateOnly AssessmentDate,
    string? ConditionNotes);