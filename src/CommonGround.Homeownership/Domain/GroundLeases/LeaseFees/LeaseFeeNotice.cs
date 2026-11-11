using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;

/// <summary>
/// A late notice the trust sent about unpaid lease fees. It keeps the balance and oldest unpaid month as of the day it
/// was sent, so the record shows what the letter said.
/// </summary>
public sealed class LeaseFeeNotice : Entity<LeaseFeeNoticeId>
{
    private LeaseFeeNotice(
        LeaseFeeNoticeId id,
        LeaseFeeNoticeLevel level,
        DateOnly sentOn,
        NoticeDelivery delivery,
        decimal balanceDue,
        DateOnly oldestUnpaidDueDate)
    {
        Id = id;
        Level = level;
        SentOn = sentOn;
        Delivery = delivery;
        BalanceDue = balanceDue;
        OldestUnpaidDueDate = oldestUnpaidDueDate;
    }

    /// <summary>Creates a notice from its recorded facts. The lease checks the rules before deciding on it.</summary>
    /// <param name="id">The notice's ID.</param>
    /// <param name="level">Which notice it is.</param>
    /// <param name="sentOn">The day it was sent.</param>
    /// <param name="delivery">How it was sent.</param>
    /// <param name="balanceDue">What the lease owed on that day.</param>
    /// <param name="oldestUnpaidDueDate">The due date of the oldest unpaid charge on that day.</param>
    public static LeaseFeeNotice Create(
        LeaseFeeNoticeId id,
        LeaseFeeNoticeLevel level,
        DateOnly sentOn,
        NoticeDelivery delivery,
        decimal balanceDue,
        DateOnly oldestUnpaidDueDate)
    {
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(delivery);

        return new LeaseFeeNotice(id, level, sentOn, delivery, balanceDue, oldestUnpaidDueDate);
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
}
