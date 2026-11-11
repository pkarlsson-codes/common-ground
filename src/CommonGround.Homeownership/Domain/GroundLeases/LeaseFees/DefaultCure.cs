using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;

/// <summary>
/// Where a notice of default stands as of a day (Model Lease Art 12.1). The homeowner cures it by paying the amount
/// the notice said was owed within the cure days after it was sent. Paying at least
/// <see cref="DefaultCurePeriod.PartialPaymentShare"/> of it within the cure days extends the deadline by the
/// extension days; that share is rounded to the cent. Every payment received on or after the day the notice was sent counts toward it.
/// </summary>
public sealed class DefaultCure : ValueObject
{
    private DefaultCure(
        DateOnly sentOn,
        decimal amountOwed,
        DateOnly originalDeadline,
        DateOnly cureBy,
        decimal paidSinceNotice,
        DateOnly? paidInFullOn,
        DefaultCureStatus status)
    {
        SentOn = sentOn;
        AmountOwed = amountOwed;
        OriginalDeadline = originalDeadline;
        CureBy = cureBy;
        PaidSinceNotice = paidSinceNotice;
        PaidInFullOn = paidInFullOn;
        Status = status;
    }

    /// <summary>Works out where a notice of default stands as of a day.</summary>
    /// <param name="sentOn">The day the notice was sent.</param>
    /// <param name="amountOwed">What the notice said was owed.</param>
    /// <param name="period">The cure period the notice was sent with.</param>
    /// <param name="payments">Every payment that counts (voided ones left out). Those before
    /// <paramref name="sentOn"/> or after <paramref name="asOf"/> are ignored.</param>
    /// <param name="asOf">The day to work it out for, usually the trust's today.</param>
    public static DefaultCure Calculate(
        DateOnly sentOn,
        decimal amountOwed,
        DefaultCurePeriod period,
        IEnumerable<ReceivedPayment> payments,
        DateOnly asOf)
    {
        ArgumentNullException.ThrowIfNull(period);
        ArgumentNullException.ThrowIfNull(payments);

        var received = payments
            .Where(p => p.ReceivedDate >= sentOn && p.ReceivedDate <= asOf)
            .OrderBy(p => p.ReceivedDate)
            .ToList();

        var originalDeadline = sentOn.AddDays(period.CureDays);
        var paidByDeadline = received.Where(p => p.ReceivedDate <= originalDeadline).Sum(p => p.Amount);
        var extended = paidByDeadline < amountOwed
            && period.ExtensionDays > 0
            && paidByDeadline >= Math.Round(amountOwed * DefaultCurePeriod.PartialPaymentShare, 2, MidpointRounding.AwayFromZero);
        var cureBy = extended ? originalDeadline.AddDays(period.ExtensionDays) : originalDeadline;

        DateOnly? paidInFullOn = null;
        var running = 0m;
        foreach (var payment in received)
        {
            running += payment.Amount;
            if (running >= amountOwed)
            {
                paidInFullOn = payment.ReceivedDate;
                break;
            }
        }

        var status = paidInFullOn is { } paid
            ? paid <= cureBy ? DefaultCureStatus.Cured : DefaultCureStatus.PaidAfterDeadline
            : asOf > cureBy ? DefaultCureStatus.Uncured : DefaultCureStatus.Curing;

        return new DefaultCure(sentOn, amountOwed, originalDeadline, cureBy, received.Sum(p => p.Amount), paidInFullOn, status);
    }

    /// <summary>Gets the day the notice was sent.</summary>
    public DateOnly SentOn { get; }

    /// <summary>Gets what the notice said was owed.</summary>
    public decimal AmountOwed { get; }

    /// <summary>Gets the deadline before any extension: the cure days after the notice was sent.</summary>
    public DateOnly OriginalDeadline { get; }

    /// <summary>Gets the last day to pay the amount owed, extended when the partial-payment share was paid in time.</summary>
    public DateOnly CureBy { get; }

    /// <summary>Gets whether the deadline was extended.</summary>
    public bool Extended => CureBy > OriginalDeadline;

    /// <summary>Gets the total received since the notice was sent.</summary>
    public decimal PaidSinceNotice { get; }

    /// <summary>Gets what is still to pay of the amount owed; zero once paid.</summary>
    public decimal StillToPay => Math.Max(0, AmountOwed - PaidSinceNotice);

    /// <summary>Gets the day the payments since the notice reached the amount owed, or null.</summary>
    public DateOnly? PaidInFullOn { get; }

    /// <summary>Gets where the notice stands.</summary>
    public DefaultCureStatus Status { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return SentOn;
        yield return AmountOwed;
        yield return OriginalDeadline;
        yield return CureBy;
        yield return PaidSinceNotice;
        yield return PaidInFullOn ?? DateOnly.MinValue;
        yield return Status;
    }
}
