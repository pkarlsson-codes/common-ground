namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to correct a recorded appraisal. Every field is the value that should stand.</summary>
/// <param name="AppraisedValue">The home's market value without the lease's restrictions.</param>
/// <param name="AppraisalDate">Date of the appraisal.</param>
/// <param name="AppraiserName">Name of the appraiser.</param>
/// <param name="Notes">Optional notes. Replaces the recorded notes.</param>
/// <param name="Reason">Why the record is corrected. Required.</param>
public sealed record CorrectAppraisalRequest(
    decimal AppraisedValue,
    DateOnly AppraisalDate,
    string AppraiserName,
    string? Notes,
    string? Reason);
