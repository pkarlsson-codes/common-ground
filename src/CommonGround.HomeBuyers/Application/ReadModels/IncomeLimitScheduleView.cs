namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>Read model for one of the trust's income limit schedules.</summary>
public class IncomeLimitScheduleView
{
    /// <summary>Gets or sets the schedule's ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the date the schedule applies from.</summary>
    public DateOnly EffectiveFrom { get; set; }

    /// <summary>Gets or sets the levels, lowest percent first.</summary>
    public List<IncomeLimitLevelEntry> Levels { get; set; } = [];

    /// <summary>Gets or sets the ID of the staff user who entered the schedule.</summary>
    public string EnteredBy { get; set; } = string.Empty;

    /// <summary>Gets or sets when the schedule was entered.</summary>
    public DateTimeOffset EnteredAt { get; set; }

    /// <summary>Gets or sets the ID of the staff user who last corrected the schedule, if anyone did.</summary>
    public string? CorrectedBy { get; set; }

    /// <summary>Gets or sets when the schedule was last corrected, if it was.</summary>
    public DateTimeOffset? CorrectedAt { get; set; }

    /// <summary>
    /// Finds the schedule in effect on <paramref name="day"/>: the latest one effective on or before it.
    /// A schedule applies from its effective date until the next schedule's.
    /// </summary>
    /// <param name="schedules">The trust's schedules, in any order.</param>
    /// <param name="day">The day to find the schedule for.</param>
    /// <returns>The schedule, or <see langword="null"/> when none is effective yet.</returns>
    public static IncomeLimitScheduleView? InEffectOn(IEnumerable<IncomeLimitScheduleView> schedules, DateOnly day) =>
        schedules.Where(s => s.EffectiveFrom <= day).MaxBy(s => s.EffectiveFrom);
}

/// <summary>One level of an <see cref="IncomeLimitScheduleView"/>.</summary>
public class IncomeLimitLevelEntry
{
    /// <summary>Gets or sets the percent of area median income.</summary>
    public int PercentOfAmi { get; set; }

    /// <summary>Gets or sets the limit for 1 to 8 people, in that order.</summary>
    public List<decimal> Limits { get; set; } = [];
}
