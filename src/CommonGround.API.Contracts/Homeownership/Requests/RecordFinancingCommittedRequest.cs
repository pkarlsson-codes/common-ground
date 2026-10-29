namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record financing committed by a lender.</summary>
public sealed record RecordFinancingCommittedRequest(
    string LenderName,
    decimal CommittedAmount,
    DateOnly PreapprovalLetterDate);
