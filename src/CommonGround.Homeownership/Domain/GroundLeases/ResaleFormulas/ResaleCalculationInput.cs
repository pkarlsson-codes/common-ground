namespace CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;

/// <summary>Runtime inputs required to calculate a resale price.</summary>
/// <param name="OriginalPurchasePrice">The price paid when the homeowner originally purchased.</param>
/// <param name="CurrentAppraisalValue">The current appraised market value.</param>
/// <param name="OriginalAppraisalValue">The appraised value at the time of original purchase.</param>
/// <param name="OwnershipYears">Number of years the homeowner has held the property.</param>
/// <param name="ApprovedImprovements">Value of CLT-approved improvements, if any.</param>
/// <param name="CurrentIndexValue">Current value of the index (required for IndexBased formula).</param>
public sealed record ResaleCalculationInput(
    decimal OriginalPurchasePrice,
    decimal CurrentAppraisalValue,
    decimal OriginalAppraisalValue,
    int OwnershipYears,
    decimal? ApprovedImprovements = null,
    decimal? CurrentIndexValue = null);
