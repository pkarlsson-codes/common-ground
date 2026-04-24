namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record a completed home inspection.</summary>
public sealed record RecordInspectionCompletedRequest(
    string InspectionDate,
    string InspectorName,
    string? Notes);
