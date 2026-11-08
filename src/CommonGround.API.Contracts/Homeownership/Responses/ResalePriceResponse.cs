namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>What a home may sell for after an appraisal, and what the trust pays to buy it.</summary>
/// <param name="FormulaPrice">The most the lease's formula allows the home to sell for.</param>
/// <param name="PurchaseOptionPrice">The lesser of the formula price and the appraised value.</param>
public sealed record ResalePriceResponse(decimal FormulaPrice, decimal PurchaseOptionPrice);
