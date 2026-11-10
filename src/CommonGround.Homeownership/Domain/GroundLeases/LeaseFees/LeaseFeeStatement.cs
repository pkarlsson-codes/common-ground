using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;

/// <summary>
/// What a lease owes as of a day. The monthly fee is charged on the 1st of each month (Model Lease Art 5.2), from
/// the first 1st on or after the lease start, to the last 1st on or before the day an ended lease ended. Charges
/// are worked out from the terms, not stored. Payments pay the oldest charges first.
/// </summary>
public sealed class LeaseFeeStatement : ValueObject
{
    private LeaseFeeStatement(
        DateOnly asOf,
        decimal monthlyFee,
        int chargeCount,
        decimal totalPaid,
        DateOnly? oldestUnpaidDueDate)
    {
        AsOf = asOf;
        MonthlyFee = monthlyFee;
        ChargeCount = chargeCount;
        TotalPaid = totalPaid;
        OldestUnpaidDueDate = oldestUnpaidDueDate;
    }

    /// <summary>Works out the statement for a lease as of a day.</summary>
    /// <param name="terms">The lease's fee terms.</param>
    /// <param name="leaseStartDate">The day the lease started.</param>
    /// <param name="endedOn">The day the lease ended, or null while it is active.</param>
    /// <param name="paidAmounts">The amounts of every payment that counts (voided ones left out).</param>
    /// <param name="asOf">The day to work it out for, usually the trust's today.</param>
    public static LeaseFeeStatement Calculate(
        LeaseFeeTerms terms,
        DateOnly leaseStartDate,
        DateOnly? endedOn,
        IEnumerable<decimal> paidAmounts,
        DateOnly asOf)
    {
        ArgumentNullException.ThrowIfNull(terms);
        ArgumentNullException.ThrowIfNull(paidAmounts);

        var totalPaid = paidAmounts.Sum();
        var monthlyFee = terms.MonthlyFee;
        if (monthlyFee == 0)
            return new LeaseFeeStatement(asOf, monthlyFee, 0, totalPaid, null);

        var firstDue = FirstDueDateOnOrAfter(leaseStartDate);
        var lastDay = endedOn is { } ended && ended < asOf ? ended : asOf;
        var chargeCount = lastDay < firstDue
            ? 0
            : ((lastDay.Year - firstDue.Year) * 12) + lastDay.Month - firstDue.Month + 1;

        var fullyPaidCharges = (int)Math.Min(chargeCount, Math.Floor(totalPaid / monthlyFee));
        DateOnly? oldestUnpaid = fullyPaidCharges < chargeCount ? firstDue.AddMonths(fullyPaidCharges) : null;

        return new LeaseFeeStatement(asOf, monthlyFee, chargeCount, totalPaid, oldestUnpaid);
    }

    /// <summary>Gets the first 1st of a month on or after a day.</summary>
    public static DateOnly FirstDueDateOnOrAfter(DateOnly day) =>
        day.Day == 1 ? day : new DateOnly(day.Year, day.Month, 1).AddMonths(1);

    /// <summary>Gets the day the statement is worked out for.</summary>
    public DateOnly AsOf { get; }

    /// <summary>Gets the fee charged each month.</summary>
    public decimal MonthlyFee { get; }

    /// <summary>Gets how many monthly charges have fallen due.</summary>
    public int ChargeCount { get; }

    /// <summary>Gets the total charged so far.</summary>
    public decimal TotalCharged => ChargeCount * MonthlyFee;

    /// <summary>Gets the total of the payments that count.</summary>
    public decimal TotalPaid { get; }

    /// <summary>Gets what is owed: above zero is a balance due, below zero a credit.</summary>
    public decimal Balance => TotalCharged - TotalPaid;

    /// <summary>Gets the due date of the oldest charge not fully paid, or null when nothing is owed.</summary>
    public DateOnly? OldestUnpaidDueDate { get; }

    /// <summary>Gets the days since the oldest unpaid charge fell due; zero when nothing is owed.</summary>
    public int DaysOverdue => OldestUnpaidDueDate is { } due ? AsOf.DayNumber - due.DayNumber : 0;

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AsOf;
        yield return MonthlyFee;
        yield return ChargeCount;
        yield return TotalPaid;
        yield return OldestUnpaidDueDate ?? DateOnly.MinValue;
    }
}
