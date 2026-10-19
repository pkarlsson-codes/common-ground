using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.Households;

/// <summary>Household circumstances.</summary>
public sealed class HouseholdCircumstances : ValueObject
{
    private HouseholdCircumstances(bool requiresAccessibility, decimal annualIncome)
    {
        RequiresAccessibility = requiresAccessibility;
        AnnualIncome = annualIncome;
    }

    /// <summary>Creates and validates a new <see cref="HouseholdCircumstances"/>.</summary>
    public static HouseholdCircumstances Create(bool value, decimal annualIncome)
    {
        // TODO: Validate and throw
        
        return new HouseholdCircumstances(value, annualIncome);
    }

    /// <summary>Household accessibility requirement.</summary>
    public bool RequiresAccessibility { get; }

    /// <summary>Household's annual income.</summary>
    public decimal AnnualIncome { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return RequiresAccessibility;
        yield return AnnualIncome;
    }
}
