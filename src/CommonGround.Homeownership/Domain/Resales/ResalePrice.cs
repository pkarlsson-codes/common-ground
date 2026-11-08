using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;

namespace CommonGround.Homeownership.Domain.Resales;

/// <summary>What a home may sell for after an appraisal, and what the trust pays to buy it.</summary>
/// <param name="FormulaPrice">The most the lease's formula allows the home to sell for.</param>
/// <param name="PurchaseOptionPrice">The lesser of the formula price and the appraised value.</param>
public sealed record ResalePrice(decimal FormulaPrice, decimal PurchaseOptionPrice)
{
    /// <summary>Works out the prices from a lease's terms and figures and an appraisal, rounded to cents.</summary>
    public static ResalePrice Calculate(
        ResalePolicyTerms terms,
        decimal basePrice,
        decimal originalAppraisedValue,
        DateOnly leaseStartDate,
        decimal appraisedValue,
        DateOnly appraisalDate,
        decimal? approvedImprovements,
        decimal? currentIndexValue)
    {
        ArgumentNullException.ThrowIfNull(terms);

        var input = new ResaleCalculationInput(
            basePrice,
            appraisedValue,
            originalAppraisedValue,
            WholeYears(leaseStartDate, appraisalDate),
            approvedImprovements,
            currentIndexValue);

        var formulaPrice = Math.Round(ResalePriceCalculator.Calculate(terms, input), 2, MidpointRounding.AwayFromZero);

        return new ResalePrice(formulaPrice, Math.Min(formulaPrice, appraisedValue));
    }

    private static int WholeYears(DateOnly from, DateOnly to)
    {
        var years = to.Year - from.Year;
        if (to < from.AddYears(years))
            years--;

        return Math.Max(years, 0);
    }
}
