namespace CommonGround.ApplicantSelection.Domain.HomeownerApplications;

/// <summary>Mortgage prequalification issued by a lender.</summary>
public sealed record MortgagePrequalification(
    string LenderName,
    decimal ApprovedAmount,
    DateOnly ExpiryDate);