namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to calculate a resale price estimate.</summary>
public sealed record CalculateResaleEstimateRequest(
    decimal OriginalPurchasePrice,
    decimal CurrentAppraisalValue,
    decimal OriginalAppraisalValue,
    int OwnershipYears,
    decimal? ApprovedImprovements,
    decimal? CurrentIndexValue);
