using CommonGround.Homeownership.Application.Errors;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;

/// <summary>
/// The monthly lease fee a homeowner pays the trust (Model Lease Art 5.1): a land use fee for the right to use
/// the land, plus a repair reserve fee the trust holds for major repairs to the home. Either may be zero.
/// </summary>
public sealed class LeaseFeeTerms : ValueObject
{
    private LeaseFeeTerms(decimal landUseFee, decimal repairReserveFee)
    {
        LandUseFee = landUseFee;
        RepairReserveFee = repairReserveFee;
    }

    /// <summary>Gets terms with no fee, for a lease whose homeowner pays nothing.</summary>
    public static LeaseFeeTerms None { get; } = new(0m, 0m);

    /// <summary>Creates fee terms, refusing a negative fee.</summary>
    /// <param name="landUseFee">The monthly land use fee.</param>
    /// <param name="repairReserveFee">The monthly repair reserve fee.</param>
    public static LeaseFeeTerms Create(decimal landUseFee, decimal repairReserveFee)
    {
        if (landUseFee < 0)
            throw BusinessRuleViolation.LeaseFeeTerms.Create.LandUseFeeNegative(landUseFee);

        if (repairReserveFee < 0)
            throw BusinessRuleViolation.LeaseFeeTerms.Create.RepairReserveFeeNegative(repairReserveFee);

        return new LeaseFeeTerms(landUseFee, repairReserveFee);
    }

    /// <summary>Gets the monthly land use fee.</summary>
    public decimal LandUseFee { get; }

    /// <summary>Gets the monthly repair reserve fee.</summary>
    public decimal RepairReserveFee { get; }

    /// <summary>Gets the total charged each month.</summary>
    public decimal MonthlyFee => LandUseFee + RepairReserveFee;

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return LandUseFee;
        yield return RepairReserveFee;
    }
}
