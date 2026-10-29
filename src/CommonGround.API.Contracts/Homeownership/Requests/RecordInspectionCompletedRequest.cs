namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record a completed home inspection.</summary>
public sealed record RecordInspectionCompletedRequest(
    DateOnly InspectionDate,
    string InspectorName,
    string? Notes);
