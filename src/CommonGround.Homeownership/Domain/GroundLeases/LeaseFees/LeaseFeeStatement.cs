using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;

/// <summary>
/// What a lease owes as of a day. The monthly fee is charged on the 1st of each month (Model Lease Art 5.2), from
/// the first 1st on or after the day fees are tracked from (usually the lease start), to the last 1st on or before
/// the day an ended lease ended. An opening balance is a charge due on the tracked-from day, older than any monthly
/// charge. Charges are worked out from the terms, not stored. Payments pay the oldest charges first, on the day they were received,
/// and any left over pays interest.
/// </summary>
/// <remarks>
/// Interest (Art 5.7) is simple interest at the lease's yearly rate over a 365-day year, on each charge's unpaid
/// amount, from its due date to the day it was paid. A part paid within the waiver days owes none. A part still unpaid
/// within the waiver days is accruing: shown, but not owed unless it stays unpaid. Interest is rounded to the cent once
/// per charge (accruing interest once in all), earns no interest itself, and stops on the day an ended lease ended.
/// </remarks>
public sealed class LeaseFeeStatement : ValueObject
{
    private LeaseFeeStatement(
        DateOnly asOf,
        Money monthlyFee,
        Money openingBalance,
        int chargeCount,
        Money totalPaid,
        DateOnly? oldestUnpaidDueDate,
        DateOnly? arrearsSince,
        LeaseFeeInterest interest)
    {
        AsOf = asOf;
        MonthlyFee = monthlyFee;
        OpeningBalance = openingBalance;
        ChargeCount = chargeCount;
        TotalPaid = totalPaid;
        OldestUnpaidDueDate = oldestUnpaidDueDate;
        ArrearsSince = arrearsSince;
        InterestCharged = interest.Charged;
        InterestWaived = interest.Waived;
        InterestAccruing = interest.Accruing;
        InterestWaivedIfPaidBy = interest.WaivedIfPaidBy;
    }

    /// <summary>Works out the statement for a lease as of a day.</summary>
    /// <param name="terms">The lease's fee terms.</param>
    /// <param name="tracking">The day the lease's fees are tracked from and what was owed then.</param>
    /// <param name="endedOn">The day the lease ended, or null while it is active.</param>
    /// <param name="payments">Every payment that counts (voided ones left out). Those received after
    /// <paramref name="asOf"/> are ignored.</param>
    /// <param name="asOf">The day to work it out for, usually the trust's today.</param>
    /// <param name="waivers">The interest the trust waived. Those after <paramref name="asOf"/> are ignored.</param>
    public static LeaseFeeStatement Calculate(
        LeaseFeeTerms terms,
        LeaseFeeTracking tracking,
        DateOnly? endedOn,
        IEnumerable<ReceivedPayment> payments,
        DateOnly asOf,
        IEnumerable<WaivedInterest>? waivers = null)
    {
        ArgumentNullException.ThrowIfNull(terms);
        ArgumentNullException.ThrowIfNull(tracking);
        ArgumentNullException.ThrowIfNull(payments);

        var received = payments.Where(p => p.ReceivedDate <= asOf).OrderBy(p => p.ReceivedDate).ToList();
        var totalPaid = received.Sum(p => p.Amount);
        var waived = (waivers ?? []).Where(w => w.WaivedOn <= asOf).Sum(w => w.Amount);
        var monthlyFee = terms.MonthlyFee;
        var lastDay = endedOn is { } ended && ended < asOf ? ended : asOf;

        var charges = new List<Charge>();
        var openingBalance = tracking.OpeningBalance.IsPositive && tracking.TrackedFrom <= lastDay
            ? tracking.OpeningBalance
            : Money.Zero;
        if (openingBalance.IsPositive)
            charges.Add(new Charge(tracking.TrackedFrom, openingBalance));

        var firstDue = FirstDueDateOnOrAfter(tracking.TrackedFrom);
        var chargeCount = monthlyFee.IsZero || lastDay < firstDue
            ? 0
            : ((lastDay.Year - firstDue.Year) * 12) + lastDay.Month - firstDue.Month + 1;
        for (var month = 0; month < chargeCount; month++)
            charges.Add(new Charge(firstDue.AddMonths(month), monthlyFee));

        var oldestUnpaid = OldestUnpaid(charges, totalPaid);
        var arrearsSince = oldestUnpaid is null ? null : ArrearsBegan(charges, received);
        var interest = terms.ChargesInterest
            ? InterestOn(terms, charges, received, lastDay, asOf) with { Waived = waived }
            : LeaseFeeInterest.None with { Waived = waived };
        if (interest.Charged.IsPositive || interest.Accruing.IsPositive)
        {
            var waiverEnds = oldestUnpaid?.AddDays(terms.InterestWaiverDays);
            interest = interest with { WaivedIfPaidBy = waiverEnds >= asOf ? waiverEnds : null };
        }

        return new LeaseFeeStatement(
            asOf, monthlyFee, openingBalance, chargeCount, totalPaid, oldestUnpaid, arrearsSince, interest);
    }

    // Payments pay the oldest charges first, so the first charge the total paid doesn't cover is the oldest unpaid.
    private static DateOnly? OldestUnpaid(List<Charge> charges, Money totalPaid)
    {
        var left = totalPaid;
        foreach (var charge in charges)
        {
            if (left < charge.Amount)
                return charge.Due;

            left -= charge.Amount;
        }

        return null;
    }

    // Pays each charge from the payments in date order, as the balance does, and charges interest on each part by
    // the day it was paid. The part still unpaid runs to the last day: accruing within the waiver days, owed after.
    // Interest is worked out in fractions of a cent and rounded once per charge.
    private static LeaseFeeInterest InterestOn(
        LeaseFeeTerms terms,
        List<Charge> charges,
        List<ReceivedPayment> received,
        DateOnly lastDay,
        DateOnly asOf)
    {
        var dailyRate = terms.LateInterestRate / 100m / 365m;
        var waiverDays = terms.InterestWaiverDays;
        var charged = Money.Zero;
        var accruing = 0m;
        var next = 0;
        var left = received.Count > 0 ? received[0].Amount : Money.Zero;
        foreach (var (due, amount) in charges)
        {
            var unpaid = amount;
            var onCharge = 0m;
            while (unpaid.IsPositive && next < received.Count)
            {
                var part = Money.Min(left, unpaid);
                var daysLate = received[next].ReceivedDate.DayNumber - due.DayNumber;
                if (daysLate > waiverDays)
                    onCharge += part.Amount * dailyRate * daysLate;

                unpaid -= part;
                left -= part;
                if (left.IsZero && ++next < received.Count)
                    left = received[next].Amount;
            }

            if (unpaid.IsPositive)
            {
                var daysUnpaid = Math.Max(0, lastDay.DayNumber - due.DayNumber);
                if (asOf.DayNumber - due.DayNumber > waiverDays)
                    onCharge += unpaid.Amount * dailyRate * daysUnpaid;
                else
                    accruing += unpaid.Amount * dailyRate * daysUnpaid;
            }

            charged += Money.RoundedFrom(onCharge);
        }

        return LeaseFeeInterest.None with { Charged = charged, Accruing = Money.RoundedFrom(accruing) };
    }

    // Walks charges and payments in date order; a charge on the same day as a payment comes first, so a payment
    // received on the 1st pays that month. The arrears begin at the first charge that leaves a balance due after
    // the lease was last paid up, and end whenever a payment brings the balance to zero or below.
    private static DateOnly? ArrearsBegan(List<Charge> charges, List<ReceivedPayment> received)
    {
        var balance = Money.Zero;
        DateOnly? since = null;
        var next = 0;
        foreach (var (due, amount) in charges)
        {
            for (; next < received.Count && received[next].ReceivedDate < due; next++)
            {
                balance -= received[next].Amount;
                if (!balance.IsPositive)
                    since = null;
            }

            balance += amount;
            if (balance.IsPositive && since is null)
                since = due;
        }

        for (; next < received.Count; next++)
        {
            balance -= received[next].Amount;
            if (!balance.IsPositive)
                since = null;
        }

        return since;
    }

    /// <summary>Gets the first 1st of a month on or after a day.</summary>
    public static DateOnly FirstDueDateOnOrAfter(DateOnly day) =>
        day.Day == 1 ? day : new DateOnly(day.Year, day.Month, 1).AddMonths(1);

    /// <summary>Gets the day the statement is worked out for.</summary>
    public DateOnly AsOf { get; }

    /// <summary>Gets the fee charged each month.</summary>
    public Money MonthlyFee { get; }

    /// <summary>Gets the balance owed on the day fees are tracked from, once that day has come; otherwise zero.</summary>
    public Money OpeningBalance { get; }

    /// <summary>Gets how many monthly charges have fallen due.</summary>
    public int ChargeCount { get; }

    /// <summary>Gets the total charged so far, the opening balance included.</summary>
    public Money TotalCharged => OpeningBalance + (ChargeCount * MonthlyFee);

    /// <summary>Gets the total of the payments that count.</summary>
    public Money TotalPaid { get; }

    /// <summary>Gets what is owed, interest included: above zero is a balance due, below zero a credit.</summary>
    public Money Balance => TotalCharged + InterestDue - TotalPaid;

    /// <summary>Gets the interest owed on fees paid late or still unpaid past the waiver days, before waivers.</summary>
    public Money InterestCharged { get; }

    /// <summary>Gets the interest the trust waived.</summary>
    public Money InterestWaived { get; }

    /// <summary>Gets the interest owed after waivers, never below zero. Part of <see cref="Balance"/>.</summary>
    public Money InterestDue => Money.Max(Money.Zero, InterestCharged - InterestWaived);

    /// <summary>
    /// Gets the interest running on fees still unpaid within the waiver days. Not owed, and not in
    /// <see cref="Balance"/>, unless the fee stays unpaid past them.
    /// </summary>
    public Money InterestAccruing { get; }

    /// <summary>
    /// Gets the last day the oldest unpaid fee can be paid without interest, while that day is today or later and
    /// the lease charges interest; otherwise null.
    /// </summary>
    public DateOnly? InterestWaivedIfPaidBy { get; }

    /// <summary>Gets the due date of the oldest charge not fully paid, or null when nothing is owed.</summary>
    public DateOnly? OldestUnpaidDueDate { get; }

    /// <summary>
    /// Gets the due date of the first charge left unpaid since the lease was last paid up, or null when nothing is
    /// owed. Earlier than <see cref="OldestUnpaidDueDate"/> when partial payments kept the lease behind.
    /// </summary>
    public DateOnly? ArrearsSince { get; }

    /// <summary>Gets the days since the oldest unpaid charge fell due; zero when nothing is owed.</summary>
    public int DaysOverdue => OldestUnpaidDueDate is { } due ? AsOf.DayNumber - due.DayNumber : 0;

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AsOf;
        yield return MonthlyFee;
        yield return OpeningBalance;
        yield return ChargeCount;
        yield return TotalPaid;
        yield return OldestUnpaidDueDate ?? DateOnly.MinValue;
        yield return ArrearsSince ?? DateOnly.MinValue;
        yield return InterestCharged;
        yield return InterestWaived;
        yield return InterestAccruing;
        yield return InterestWaivedIfPaidBy ?? DateOnly.MinValue;
    }
}

/// <summary>The interest parts of a statement, while it is worked out.</summary>
internal readonly record struct LeaseFeeInterest(Money Charged, Money Waived, Money Accruing, DateOnly? WaivedIfPaidBy)
{
    public static LeaseFeeInterest None => new(Money.Zero, Money.Zero, Money.Zero, null);
}

/// <summary>An amount that fell due on a day, while a statement is worked out.</summary>
internal readonly record struct Charge(DateOnly Due, Money Amount);
