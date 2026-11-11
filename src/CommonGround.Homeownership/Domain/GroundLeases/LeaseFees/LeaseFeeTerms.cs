using CommonGround.Homeownership.Application.Errors;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;

/// <summary>
/// The monthly lease fee a homeowner pays the trust (Model Lease Art 5.1): a land use fee for the right to use
/// the land, plus a repair reserve fee the trust holds for major repairs to the home. Either may be zero. Also the
/// interest the trust charges on a late fee (Art 5.7), whose rate each trust writes into its lease.
/// </summary>
public sealed class LeaseFeeTerms : ValueObject
{
    /// <summary>The days after the due date within which a payment owes no interest, as in Art 5.7.</summary>
    public const int DefaultInterestWaiverDays = 30;

    /// <summary>The highest annual interest rate accepted, in percent. A guard against typing errors, not a legal limit.</summary>
    public const decimal MaxLateInterestRate = 100m;

    private LeaseFeeTerms(decimal landUseFee, decimal repairReserveFee, decimal lateInterestRate, int interestWaiverDays)
    {
        LandUseFee = landUseFee;
        RepairReserveFee = repairReserveFee;
        LateInterestRate = lateInterestRate;
        InterestWaiverDays = interestWaiverDays;
    }

    /// <summary>Gets terms with no fee, for a lease whose homeowner pays nothing.</summary>
    public static LeaseFeeTerms None { get; } = new(0m, 0m, 0m, DefaultInterestWaiverDays);

    /// <summary>
    /// Creates fee terms. Refused for a negative fee, an interest rate below zero or above
    /// <see cref="MaxLateInterestRate"/>, or negative waiver days.
    /// </summary>
    /// <param name="landUseFee">The monthly land use fee.</param>
    /// <param name="repairReserveFee">The monthly repair reserve fee.</param>
    /// <param name="lateInterestRate">The yearly interest rate on a late fee, in percent; zero charges none.</param>
    /// <param name="interestWaiverDays">The days after the due date within which a payment owes no interest.</param>
    public static LeaseFeeTerms Create(
        decimal landUseFee,
        decimal repairReserveFee,
        decimal lateInterestRate = 0m,
        int interestWaiverDays = DefaultInterestWaiverDays)
    {
        if (landUseFee < 0)
            throw BusinessRuleViolation.LeaseFeeTerms.Create.LandUseFeeNegative(landUseFee);

        if (repairReserveFee < 0)
            throw BusinessRuleViolation.LeaseFeeTerms.Create.RepairReserveFeeNegative(repairReserveFee);

        if (lateInterestRate is < 0 or > MaxLateInterestRate)
            throw BusinessRuleViolation.LeaseFeeTerms.Create.LateInterestRateOutOfRange(lateInterestRate);

        if (interestWaiverDays < 0)
            throw BusinessRuleViolation.LeaseFeeTerms.Create.InterestWaiverDaysNegative(interestWaiverDays);

        return new LeaseFeeTerms(landUseFee, repairReserveFee, lateInterestRate, interestWaiverDays);
    }

    /// <summary>Gets the monthly land use fee.</summary>
    public decimal LandUseFee { get; }

    /// <summary>Gets the monthly repair reserve fee.</summary>
    public decimal RepairReserveFee { get; }

    /// <summary>Gets the total charged each month.</summary>
    public decimal MonthlyFee => LandUseFee + RepairReserveFee;

    /// <summary>Gets the yearly interest rate on a late fee, in percent (10 is 10% a year). Zero charges no interest.</summary>
    public decimal LateInterestRate { get; }

    /// <summary>
    /// Gets the days after a fee's due date within which paying it owes no interest. Paid later, interest runs from
    /// the due date.
    /// </summary>
    public int InterestWaiverDays { get; }

    /// <summary>Gets whether a late fee earns interest.</summary>
    public bool ChargesInterest => LateInterestRate > 0;

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return LandUseFee;
        yield return RepairReserveFee;
        yield return LateInterestRate;
        yield return InterestWaiverDays;
    }
}
