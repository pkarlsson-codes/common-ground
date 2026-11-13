using CommonGround.SharedKernel.Domain;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases;

/// <summary>
/// An unpaid notice of default on a lease's fee ledger (Model Lease Art 12.1), as it stood when a case was opened:
/// the day it was sent, what it said was owed, the day the homeowner had to pay by, and what was still to pay.
/// </summary>
public sealed class LeaseDefaultNotice : ValueObject
{
    private LeaseDefaultNotice(DateOnly sentOn, Money amountOwed, DateOnly cureBy, Money stillToPay)
    {
        SentOn = sentOn;
        AmountOwed = amountOwed;
        CureBy = cureBy;
        StillToPay = stillToPay;
    }

    /// <summary>Creates a <see cref="LeaseDefaultNotice"/>. The figures come from the ledger, never from a user.</summary>
    /// <param name="sentOn">The day the notice was sent.</param>
    /// <param name="amountOwed">What the notice said was owed.</param>
    /// <param name="cureBy">The last day to pay the amount owed, after any extension.</param>
    /// <param name="stillToPay">What was still to pay of the amount owed; more than zero and at most the amount owed.</param>
    public static LeaseDefaultNotice Create(DateOnly sentOn, Money amountOwed, DateOnly cureBy, Money stillToPay)
    {
        ArgumentNullException.ThrowIfNull(amountOwed);
        ArgumentNullException.ThrowIfNull(stillToPay);
        if (!stillToPay.IsPositive)
            throw new ArgumentOutOfRangeException(nameof(stillToPay), stillToPay.Amount, "An unpaid notice has something still to pay.");
        if (stillToPay > amountOwed)
            throw new ArgumentOutOfRangeException(nameof(stillToPay), stillToPay.Amount, "Still to pay can't exceed the amount owed.");
        if (cureBy < sentOn)
            throw new ArgumentOutOfRangeException(nameof(cureBy), cureBy, "The cure deadline can't be before the notice was sent.");

        return new LeaseDefaultNotice(sentOn, amountOwed, cureBy, stillToPay);
    }

    /// <summary>Gets the day the notice was sent.</summary>
    public DateOnly SentOn { get; }

    /// <summary>Gets what the notice said was owed.</summary>
    public Money AmountOwed { get; }

    /// <summary>Gets the last day to pay the amount owed.</summary>
    public DateOnly CureBy { get; }

    /// <summary>Gets what was still to pay of the amount owed.</summary>
    public Money StillToPay { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return SentOn;
        yield return AmountOwed;
        yield return CureBy;
        yield return StillToPay;
    }
}
