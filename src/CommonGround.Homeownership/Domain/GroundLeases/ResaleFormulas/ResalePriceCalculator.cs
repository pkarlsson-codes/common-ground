using CommonGround.Homeownership.Application.Errors;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;

/// <summary>Calculates the resale price based on policy terms and runtime inputs.</summary>
public static class ResalePriceCalculator
{
    /// <summary>Calculates the maximum allowable resale price.</summary>
    /// <exception cref="BusinessRuleViolationException">Thrown when an index-based price is calculated without a current index value.</exception>
    public static decimal Calculate(ResalePolicyTerms terms, ResaleCalculationInput input)
    {
        ArgumentNullException.ThrowIfNull(terms);
        ArgumentNullException.ThrowIfNull(input);

        var basePrice = terms.FormulaKind switch
        {
            FormulaKind.AppraisalBased => CalculateAppraisalBased(terms, input),
            FormulaKind.IndexBased => CalculateIndexBased(terms, input),
            FormulaKind.FixedPercentage => CalculateFixedPercentage(terms, input),
            _ => throw new InvalidOperationException($"Unknown formula kind: {terms.FormulaKind}")
        };

        if (terms.ImprovementsCap.HasValue && input.ApprovedImprovements is not null)
        {
            var credit = Math.Min(input.ApprovedImprovements.Amount, terms.ImprovementsCap.Value);
            return basePrice + credit;
        }

        return basePrice;
    }

    private static decimal CalculateAppraisalBased(ResalePolicyTerms terms, ResaleCalculationInput input)
    {
        var appreciation = (input.CurrentAppraisalValue - input.OriginalAppraisalValue).Amount;
        return input.OriginalPurchasePrice.Amount + (appreciation * terms.AppreciationShare!.Value);
    }

    private static decimal CalculateIndexBased(ResalePolicyTerms terms, ResaleCalculationInput input)
    {
        if (input.CurrentIndexValue is not { } currentIndexValue)
            throw BusinessRuleViolation.ResalePolicyTerms.IndexBased.CurrentIndexValueRequired();

        return input.OriginalPurchasePrice.Amount * (currentIndexValue / terms.BaselineIndexValue!.Value);
    }

    private static decimal CalculateFixedPercentage(ResalePolicyTerms terms, ResaleCalculationInput input)
    {
        var factor = (decimal)Math.Pow((double)(1 + terms.AnnualPercentage!.Value), input.OwnershipYears);
        return input.OriginalPurchasePrice.Amount * factor;
    }
}
