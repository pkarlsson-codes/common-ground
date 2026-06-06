namespace CommonGround.API.Contracts.ApplicantSelection.Requests;

/// <summary>Request to record mortgage prequalification.</summary>
public sealed record RecordMortgagePrequalificationRequest(
    string LenderName,
    decimal ApprovedAmount,
    string ExpiryDate);