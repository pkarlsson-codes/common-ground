namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record the home's appraisal.</summary>
public sealed record RecordAppraisalCompletedRequest(
    decimal AppraisedValue,
    DateOnly AppraisalDate,
    string AppraiserName,
    string? Notes);
