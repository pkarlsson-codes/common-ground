using CommonGround.Homeownership.Application.Errors;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;

/// <summary>
/// The day the trust began tracking a lease's fees in CommonGround, and what the homeowner owed on that day. A lease
/// signed before the trust used CommonGround is tracked from a later day than its start, so the years before don't
/// show as unpaid. The opening balance is due on the tracked-from day; monthly charges begin at the first 1st on or
/// after it.
/// </summary>
public sealed class LeaseFeeTracking : ValueObject
{
    private LeaseFeeTracking(DateOnly trackedFrom, Money openingBalance)
    {
        TrackedFrom = trackedFrom;
        OpeningBalance = openingBalance;
    }

    /// <summary>Gets tracking from the day a lease started with nothing owed, as for a lease new to the homeowner.</summary>
    public static LeaseFeeTracking FromLeaseStart(DateOnly leaseStartDate) => new(leaseStartDate, Money.Zero);

    /// <summary>
    /// Creates the tracking for a lease. Refused for a tracked-from day before the lease started, or an opening
    /// balance below zero.
    /// </summary>
    /// <param name="trackedFrom">The first day the trust tracks fees in CommonGround.</param>
    /// <param name="openingBalance">What the homeowner owed on that day, interest included; zero when paid up.</param>
    /// <param name="leaseStartDate">The day the lease started.</param>
    public static LeaseFeeTracking Create(DateOnly trackedFrom, Money openingBalance, DateOnly leaseStartDate)
    {
        ArgumentNullException.ThrowIfNull(openingBalance);

        if (trackedFrom < leaseStartDate)
            throw BusinessRuleViolation.LeaseFeeTracking.Create.TrackedFromBeforeLeaseStart(trackedFrom, leaseStartDate);

        if (openingBalance.IsNegative)
            throw BusinessRuleViolation.LeaseFeeTracking.Create.OpeningBalanceNegative(openingBalance.Amount);

        return new LeaseFeeTracking(trackedFrom, openingBalance);
    }

    /// <summary>Gets the first day the trust tracks fees in CommonGround.</summary>
    public DateOnly TrackedFrom { get; }

    /// <summary>Gets what the homeowner owed on <see cref="TrackedFrom"/>, due that day.</summary>
    public Money OpeningBalance { get; }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TrackedFrom;
        yield return OpeningBalance;
    }
}
