namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.TrackDetails;

/// <summary>Details recorded when a home inspection is completed.</summary>
/// <param name="InspectionDate">Date of the inspection.</param>
/// <param name="InspectorName">Name of the inspector.</param>
/// <param name="Notes">Optional inspection notes.</param>
public sealed record InspectionResult(
    DateOnly InspectionDate, string InspectorName, string? Notes);
