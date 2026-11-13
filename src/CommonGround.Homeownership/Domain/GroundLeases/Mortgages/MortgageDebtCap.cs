using CommonGround.Homeownership.Application.Errors;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.Mortgages;

/// <summary>
/// The most total mortgage debt on a home may be when the trust permits a new loan, as a whole percent of the
/// purchase option price (Model Lease Art 8.3). The manual leaves the figure to each trust; CLTs use 80 to 100%.
/// </summary>
public sealed class MortgageDebtCap : ValueObject
{
    /// <summary>The lowest cap a lease can set.</summary>
    public const int MinPercent = 1;

    /// <summary>The highest cap a lease can set.</summary>
    public const int MaxPercent = 200;

    private MortgageDebtCap(int percent)
    {
        Percent = percent;
    }

    /// <summary>Creates a cap, refusing a percent outside 1 to 200.</summary>
    /// <param name="percent">Whole percent of the purchase option price.</param>
    public static MortgageDebtCap Create(int percent)
    {
        if (percent is < MinPercent or > MaxPercent)
            throw BusinessRuleViolation.MortgageDebtCap.Create.PercentOutOfRange(percent);

        return new MortgageDebtCap(percent);
    }

    /// <summary>Gets the cap as a whole percent of the purchase option price.</summary>
    public int Percent { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Percent;
    }
}
