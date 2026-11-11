using CommonGround.Homeownership.Application.Errors;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;

/// <summary>
/// How many days overdue a lease fee may be before the trust sends each late notice. Ch 23 asks for a first notice
/// "by the middle of that first month" and a second once the fee is more than a month late; each trust sets its own.
/// The notice of default (Art 12.1) follows the second.
/// </summary>
public sealed class LeaseFeeNoticeSchedule : ValueObject
{
    /// <summary>The first notice's days when a trust hasn't set its own.</summary>
    public const int DefaultFirstNoticeDays = 10;

    /// <summary>The second notice's days when a trust hasn't set its own.</summary>
    public const int DefaultSecondNoticeDays = 30;

    /// <summary>The notice of default's days when a trust hasn't set its own.</summary>
    public const int DefaultDefaultNoticeDays = 60;

    private LeaseFeeNoticeSchedule(int firstNoticeDays, int secondNoticeDays, int defaultNoticeDays)
    {
        FirstNoticeDays = firstNoticeDays;
        SecondNoticeDays = secondNoticeDays;
        DefaultNoticeDays = defaultNoticeDays;
    }

    /// <summary>Gets the schedule a trust has until it sets its own: 10, 30 and 60 days.</summary>
    public static LeaseFeeNoticeSchedule Default { get; } =
        new(DefaultFirstNoticeDays, DefaultSecondNoticeDays, DefaultDefaultNoticeDays);

    /// <summary>
    /// Creates a schedule. Refused when the first notice's days are below 1, the second's are not more than the
    /// first's, or the notice of default's are not more than the second's.
    /// </summary>
    public static LeaseFeeNoticeSchedule Create(int firstNoticeDays, int secondNoticeDays, int defaultNoticeDays)
    {
        if (firstNoticeDays < 1)
            throw BusinessRuleViolation.LeaseFeeNoticeSchedule.Create.FirstNoticeDaysTooFew(firstNoticeDays);

        if (secondNoticeDays <= firstNoticeDays)
            throw BusinessRuleViolation.LeaseFeeNoticeSchedule.Create.SecondNoticeNotAfterFirst(firstNoticeDays, secondNoticeDays);

        if (defaultNoticeDays <= secondNoticeDays)
            throw BusinessRuleViolation.LeaseFeeNoticeSchedule.Create.DefaultNoticeNotAfterSecond(secondNoticeDays, defaultNoticeDays);

        return new LeaseFeeNoticeSchedule(firstNoticeDays, secondNoticeDays, defaultNoticeDays);
    }

    /// <summary>Gets the days overdue before the first notice.</summary>
    public int FirstNoticeDays { get; }

    /// <summary>Gets the days overdue before the second notice.</summary>
    public int SecondNoticeDays { get; }

    /// <summary>Gets the days overdue before the notice of default.</summary>
    public int DefaultNoticeDays { get; }

    /// <summary>Gets the days overdue before a notice of the given level.</summary>
    public int DaysFor(LeaseFeeNoticeLevel level)
    {
        ArgumentNullException.ThrowIfNull(level);

        return level == LeaseFeeNoticeLevel.Default ? DefaultNoticeDays
            : level == LeaseFeeNoticeLevel.Second ? SecondNoticeDays
            : FirstNoticeDays;
    }

    /// <summary>
    /// Gets the notice a lease needs now, or null. A notice is due once the oldest unpaid charge has been overdue for
    /// its days, unless a notice of that level or higher was sent since the arrears began. Past a later notice's days,
    /// it is due even if the earlier ones were never sent.
    /// </summary>
    /// <param name="statement">The lease's statement as of today.</param>
    /// <param name="sent">The notices sent on the lease.</param>
    public LeaseFeeNoticeLevel? NoticeDue(LeaseFeeStatement statement, IEnumerable<LeaseFeeNoticeSent> sent)
    {
        ArgumentNullException.ThrowIfNull(statement);
        ArgumentNullException.ThrowIfNull(sent);

        if (statement.Balance <= 0 || statement.ArrearsSince is not { } arrearsSince)
            return null;

        var reached = statement.DaysOverdue >= DefaultNoticeDays ? LeaseFeeNoticeLevel.Default
            : statement.DaysOverdue >= SecondNoticeDays ? LeaseFeeNoticeLevel.Second
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
        yield return DefaultNoticeDays;
    }
}

/// <summary>A notice already sent, as <see cref="LeaseFeeNoticeSchedule.NoticeDue"/> needs it.</summary>
/// <param name="Level">Which notice it was.</param>
/// <param name="SentOn">The day it was sent.</param>
public readonly record struct LeaseFeeNoticeSent(LeaseFeeNoticeLevel Level, DateOnly SentOn);
