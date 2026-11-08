namespace CommonGround.Homeownership.Domain.Resales;

/// <summary>What a resale knows of its latest appraisal.</summary>
/// <param name="AppraisedValue">The home's appraised market value.</param>
/// <param name="AppraisalDate">The day of the appraisal.</param>
/// <param name="FormulaPrice">The most the lease's formula allows the home to sell for.</param>
/// <param name="PurchaseOptionPrice">What the trust pays to buy the home.</param>
public sealed record ResaleAppraisal(decimal AppraisedValue, DateOnly AppraisalDate, decimal FormulaPrice, decimal PurchaseOptionPrice);
