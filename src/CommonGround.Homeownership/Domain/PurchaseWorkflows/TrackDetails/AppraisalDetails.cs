namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.TrackDetails;

/// <summary>Details recorded when the appraisal is completed.</summary>
/// <param name="AppraisedValue">The home's market value without the lease's restrictions.</param>
/// <param name="AppraisalDate">Date of the appraisal.</param>
/// <param name="AppraiserName">Name of the appraiser.</param>
/// <param name="Notes">Optional notes.</param>
public sealed record AppraisalDetails(
    decimal AppraisedValue, DateOnly AppraisalDate, string AppraiserName, string? Notes);
