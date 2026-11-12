using CommonGround.HomeBuyers.Application.Errors;
using CommonGround.HomeBuyers.Domain.IncomeLimitSchedules.Events;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.IncomeLimitSchedules;

/// <summary>
/// The income limits a trust is bound by, as HUD or the state agency published them, from an effective date
/// until the next schedule's. Each level is a percent of area median income with a limit per household size.
/// </summary>
public sealed class IncomeLimitSchedule : AggregateRoot<IncomeLimitScheduleId>
{
#pragma warning disable CS8618 // State is set by Create from the stream's first event.
    private IncomeLimitSchedule() { }
#pragma warning restore CS8618

    /// <summary>Gets the date the schedule applies from.</summary>
    public DateOnly EffectiveFrom { get; private set; }

    /// <summary>Gets the levels, lowest percent first.</summary>
    public IReadOnlyList<IncomeLimitLevel> Levels { get; private set; }

    /// <summary>Decides to enter a schedule.</summary>
    /// <param name="id">The new schedule's ID.</param>
    /// <param name="effectiveFrom">The date it applies from.</param>
    /// <param name="levels">At least one level, no two with the same percent.</param>
    /// <param name="scheduleOnSameDate">Another of the trust's schedules effective from the same date, if any.</param>
    /// <param name="enteredBy">The staff user entering it.</param>
    /// <returns>The entry.</returns>
    public static IncomeLimitScheduleEntered Enter(
        IncomeLimitScheduleId id,
        DateOnly effectiveFrom,
        IReadOnlyList<IncomeLimitLevel> levels,
        IncomeLimitScheduleId? scheduleOnSameDate,
        UserId enteredBy)
    {
        var sorted = Validate(effectiveFrom, levels, scheduleOnSameDate);
        return new IncomeLimitScheduleEntered(id.Value, effectiveFrom, ToEntries(sorted), enteredBy.Value);
    }

    /// <summary>Decides to replace the schedule's effective date and levels. No-op when nothing changed.</summary>
    /// <param name="effectiveFrom">The corrected date it applies from.</param>
    /// <param name="levels">At least one level, no two with the same percent.</param>
    /// <param name="scheduleOnSameDate">Another of the trust's schedules effective from the same date, if any.</param>
    /// <param name="correctedBy">The staff user correcting it.</param>
    /// <returns>The correction, or <see langword="null"/> when nothing changed.</returns>
    public IncomeLimitScheduleCorrected? Correct(
        DateOnly effectiveFrom,
        IReadOnlyList<IncomeLimitLevel> levels,
        IncomeLimitScheduleId? scheduleOnSameDate,
        UserId correctedBy)
    {
        var sorted = Validate(effectiveFrom, levels, scheduleOnSameDate == Id ? null : scheduleOnSameDate);
        if (effectiveFrom == EffectiveFrom && sorted.SequenceEqual(Levels))
        {
            return null;
        }

        return new IncomeLimitScheduleCorrected(Id.Value, effectiveFrom, ToEntries(sorted), correctedBy.Value);
    }

    /// <summary>Gets the level for <paramref name="percentOfAmi"/>, or <see langword="null"/> when the schedule has none.</summary>
    /// <param name="percentOfAmi">The percent of area median income.</param>
    /// <returns>The level, if the schedule has it.</returns>
    public IncomeLimitLevel? LevelFor(int percentOfAmi) =>
        Levels.FirstOrDefault(l => l.PercentOfAmi == percentOfAmi);

    /// <summary>Evolves a schedule from its stream's first event.</summary>
    public static IncomeLimitSchedule Create(IncomeLimitScheduleEntered e)
    {
        ArgumentNullException.ThrowIfNull(e);

        return new IncomeLimitSchedule
        {
            Id = IncomeLimitScheduleId.Create(e.AggregateId),
            EffectiveFrom = e.EffectiveFrom,
            Levels = ToLevels(e.Levels),
        };
    }

    /// <summary>Applies <see cref="IncomeLimitScheduleCorrected"/>.</summary>
    public void Apply(IncomeLimitScheduleCorrected e)
    {
        ArgumentNullException.ThrowIfNull(e);

        EffectiveFrom = e.EffectiveFrom;
        Levels = ToLevels(e.Levels);
    }

    private static List<IncomeLimitLevel> Validate(
        DateOnly effectiveFrom,
        IReadOnlyList<IncomeLimitLevel> levels,
        IncomeLimitScheduleId? scheduleOnSameDate)
    {
        ArgumentNullException.ThrowIfNull(levels);

        if (scheduleOnSameDate is { } taken)
        {
            throw BusinessRuleViolation.IncomeLimitSchedule.EffectiveFrom.Taken(effectiveFrom, taken);
        }

        if (levels.Count == 0)
        {
            throw BusinessRuleViolation.IncomeLimitSchedule.Levels.Required();
        }

        var duplicate = levels.GroupBy(l => l.PercentOfAmi).FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
        {
            throw BusinessRuleViolation.IncomeLimitSchedule.Levels.DuplicatePercent(duplicate.Key);
        }

        return [.. levels.OrderBy(l => l.PercentOfAmi)];
    }

    private static List<EnteredIncomeLimitLevel> ToEntries(IEnumerable<IncomeLimitLevel> levels) =>
        [.. levels.Select(l => new EnteredIncomeLimitLevel(l.PercentOfAmi, l.Limits))];

    private static List<IncomeLimitLevel> ToLevels(IEnumerable<EnteredIncomeLimitLevel> entries) =>
        [.. entries.Select(e => IncomeLimitLevel.Create(e.PercentOfAmi, e.Limits))];
}
