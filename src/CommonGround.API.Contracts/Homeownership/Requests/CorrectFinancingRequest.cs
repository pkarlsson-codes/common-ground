namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to correct a recorded financing commitment. Every field is the value that should stand.</summary>
/// <param name="LenderName">Name of the lender.</param>
/// <param name="CommittedAmount">Amount committed.</param>
/// <param name="PreapprovalLetterDate">Date of the pre-approval letter.</param>
/// <param name="Reason">Why the record is corrected. Required.</param>
public sealed record CorrectFinancingRequest(
    string LenderName,
    decimal CommittedAmount,
    DateOnly PreapprovalLetterDate,
    string? Reason);
