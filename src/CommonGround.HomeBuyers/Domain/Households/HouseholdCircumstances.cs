using CommonGround.HomeBuyers.Application.Errors;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.Households;

/// <summary>Household circumstances.</summary>
public sealed class HouseholdCircumstances : ValueObject
{
    private HouseholdCircumstances(bool requiresAccessibility, Money annualIncome)
    {
        RequiresAccessibility = requiresAccessibility;
        AnnualIncome = annualIncome;
    }

    /// <summary>Creates and validates a new <see cref="HouseholdCircumstances"/>.</summary>
    public static HouseholdCircumstances Create(bool requiresAccessibility, Money annualIncome)
    {
        ArgumentNullException.ThrowIfNull(annualIncome);
        if (annualIncome.IsNegative)
            throw BusinessRuleViolation.Household.Circumstances.AnnualIncomeNegative(annualIncome);

        return new HouseholdCircumstances(requiresAccessibility, annualIncome);
    }

    /// <summary>Household accessibility requirement.</summary>
    public bool RequiresAccessibility { get; }

    /// <summary>Household's annual income.</summary>
    public Money AnnualIncome { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return RequiresAccessibility;
        yield return AnnualIncome;
    }
}
