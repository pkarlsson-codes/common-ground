using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;

/// <summary>
/// A late notice the trust sent about unpaid lease fees. It keeps the balance and oldest unpaid month as of the day it
/// was sent, so the record shows what the letter said. A notice of default also keeps the cure period it gave.
/// </summary>
public sealed class LeaseFeeNotice : Entity<LeaseFeeNoticeId>
{
    private LeaseFeeNotice(
        LeaseFeeNoticeId id,
        LeaseFeeNoticeLevel level,
        DateOnly sentOn,
        NoticeDelivery delivery,
        decimal balanceDue,
        DateOnly oldestUnpaidDueDate,
        DefaultCurePeriod? curePeriod)
    {
        Id = id;
        Level = level;
        SentOn = sentOn;
        Delivery = delivery;
        BalanceDue = balanceDue;
        OldestUnpaidDueDate = oldestUnpaidDueDate;
        CurePeriod = curePeriod;
    }

    /// <summary>Creates a notice from its recorded facts. The lease checks the rules before deciding on it.</summary>
    /// <param name="id">The notice's ID.</param>
    /// <param name="level">Which notice it is.</param>
    /// <param name="sentOn">The day it was sent.</param>
    /// <param name="delivery">How it was sent.</param>
    /// <param name="balanceDue">What the lease owed on that day.</param>
    /// <param name="oldestUnpaidDueDate">The due date of the oldest unpaid charge on that day.</param>
    /// <param name="curePeriod">The cure period a notice of default gave; null for other notices.</param>
    public static LeaseFeeNotice Create(
        LeaseFeeNoticeId id,
        LeaseFeeNoticeLevel level,
        DateOnly sentOn,
        NoticeDelivery delivery,
        decimal balanceDue,
        DateOnly oldestUnpaidDueDate,
        DefaultCurePeriod? curePeriod = null)
    {
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(delivery);

        return new LeaseFeeNotice(id, level, sentOn, delivery, balanceDue, oldestUnpaidDueDate, curePeriod);
    }

    /// <summary>Gets which notice it is.</summary>
    public LeaseFeeNoticeLevel Level { get; }

    /// <summary>Gets the day it was sent.</summary>
    public DateOnly SentOn { get; }

    /// <summary>Gets how it was sent.</summary>
    public NoticeDelivery Delivery { get; }

    /// <summary>Gets what the lease owed on the day it was sent.</summary>
    public decimal BalanceDue { get; }

    /// <summary>Gets the due date of the oldest unpaid charge on the day it was sent.</summary>
    public DateOnly OldestUnpaidDueDate { get; }

    /// <summary>Gets the cure period a notice of default gave; null for other notices.</summary>
    public DefaultCurePeriod? CurePeriod { get; }

    /// <summary>Gets where a notice of default stands as of a day; null for other notices.</summary>
    /// <param name="payments">Every payment that counts on the lease.</param>
    /// <param name="asOf">The day to work it out for.</param>
    public DefaultCure? CureAsOf(IEnumerable<ReceivedPayment> payments, DateOnly asOf) =>
        CurePeriod is { } period ? DefaultCure.Calculate(SentOn, BalanceDue, period, payments, asOf) : null;
}
