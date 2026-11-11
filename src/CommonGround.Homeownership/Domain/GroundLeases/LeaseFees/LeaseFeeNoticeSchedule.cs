using CommonGround.Homeownership.Application.Errors;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;

/// <summary>
/// How many days overdue a lease fee may be before the trust sends each late notice. Ch 23 asks for a first notice
/// "by the middle of that first month" and a second once the fee is more than a month late; each trust sets its own.
/// </summary>
public sealed class LeaseFeeNoticeSchedule : ValueObject
{
    /// <summary>The first notice's days when a trust hasn't set its own.</summary>
    public const int DefaultFirstNoticeDays = 10;

    /// <summary>The second notice's days when a trust hasn't set its own.</summary>
    public const int DefaultSecondNoticeDays = 30;

    private LeaseFeeNoticeSchedule(int firstNoticeDays, int secondNoticeDays)
    {
        FirstNoticeDays = firstNoticeDays;
        SecondNoticeDays = secondNoticeDays;
    }

    /// <summary>Gets the schedule a trust has until it sets its own: 10 and 30 days.</summary>
    public static LeaseFeeNoticeSchedule Default { get; } = new(DefaultFirstNoticeDays, DefaultSecondNoticeDays);

    /// <summary>
    /// Creates a schedule. Refused when the first notice's days are below 1, or the second's are not more than the
    /// first's.
    /// </summary>
    public static LeaseFeeNoticeSchedule Create(int firstNoticeDays, int secondNoticeDays)
    {
        if (firstNoticeDays < 1)
            throw BusinessRuleViolation.LeaseFeeNoticeSchedule.Create.FirstNoticeDaysTooFew(firstNoticeDays);

        if (secondNoticeDays <= firstNoticeDays)
            throw BusinessRuleViolation.LeaseFeeNoticeSchedule.Create.SecondNoticeNotAfterFirst(firstNoticeDays, secondNoticeDays);

        return new LeaseFeeNoticeSchedule(firstNoticeDays, secondNoticeDays);
    }

    /// <summary>Gets the days overdue before the first notice.</summary>
    public int FirstNoticeDays { get; }

    /// <summary>Gets the days overdue before the second notice.</summary>
    public int SecondNoticeDays { get; }

    /// <summary>
    /// Gets the notice a lease needs now, or null. A notice is due once the oldest unpaid charge has been overdue for
    /// its days, unless a notice of that level or higher was sent since the arrears began. Past the second notice's
    /// days, the second is due even if the first was never sent.
    /// </summary>
    /// <param name="statement">The lease's statement as of today.</param>
    /// <param name="sent">The notices sent on the lease.</param>
    public LeaseFeeNoticeLevel? NoticeDue(LeaseFeeStatement statement, IEnumerable<LeaseFeeNoticeSent> sent)
    {
        ArgumentNullException.ThrowIfNull(statement);
        ArgumentNullException.ThrowIfNull(sent);

        if (statement.Balance <= 0 || statement.ArrearsSince is not { } arrearsSince)
            return null;

        var reached = statement.DaysOverdue >= SecondNoticeDays ? LeaseFeeNoticeLevel.Second
            : statement.DaysOverdue >= FirstNoticeDays ? LeaseFeeNoticeLevel.First
            : null;
        if (reached is null)
            return null;

        var alreadySent = sent.Any(n => n.SentOn >= arrearsSince && n.Level.Value >= reached.Value);
        return alreadySent ? null : reached;
    }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstNoticeDays;
        yield return SecondNoticeDays;
    }
}

/// <summary>A notice already sent, as <see cref="LeaseFeeNoticeSchedule.NoticeDue"/> needs it.</summary>
/// <param name="Level">Which notice it was.</param>
/// <param name="SentOn">The day it was sent.</param>
public readonly record struct LeaseFeeNoticeSent(LeaseFeeNoticeLevel Level, DateOnly SentOn);
