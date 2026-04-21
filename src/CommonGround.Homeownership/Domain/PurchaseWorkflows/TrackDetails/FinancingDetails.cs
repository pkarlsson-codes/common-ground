namespace CommonGround.Homeownership.Domain.PurchaseWorkflows.TrackDetails;

/// <summary>Details recorded when financing is committed.</summary>
/// <param name="LenderName">Name of the lending institution.</param>
/// <param name="CommittedAmount">Amount committed by the lender.</param>
/// <param name="PreapprovalLetterDate">Date of the pre-approval letter.</param>
public sealed record FinancingDetails(
    string LenderName, decimal CommittedAmount, DateOnly PreapprovalLetterDate);
