namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>Request to record mortgage prequalification.</summary>
public sealed record RecordMortgagePrequalificationRequest(
    string LenderName,
    decimal ApprovedAmount,
    DateOnly ExpiryDate);