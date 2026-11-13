using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.SharedKernel.Domain;

/// <summary>
/// A copy of an income limit schedule as it stood on one day: its effective date and every level, lowest percent
/// first. A home selection keeps the one in effect when it starts, and the purchase carries it, so later
/// corrections to the schedule don't change what the purchase is judged by.
/// </summary>
public sealed class IncomeLimitTable : ValueObject
{
    private IncomeLimitTable(DateOnly effectiveFrom, IReadOnlyList<IncomeLimitLevel> levels)
    {
        EffectiveFrom = effectiveFrom;
        Levels = levels;
    }

    /// <summary>Gets the date the schedule applies from.</summary>
    public DateOnly EffectiveFrom { get; }

    /// <summary>Gets the levels, lowest percent first.</summary>
    public IReadOnlyList<IncomeLimitLevel> Levels { get; }

    /// <summary>Creates a table from a schedule's levels.</summary>
    /// <param name="effectiveFrom">The date the schedule applies from.</param>
    /// <param name="levels">At least one level, no two with the same percent, in any order.</param>
    /// <returns>The table.</returns>
    public static IncomeLimitTable Create(DateOnly effectiveFrom, IReadOnlyList<IncomeLimitLevel> levels)
    {
        ArgumentNullException.ThrowIfNull(levels);

        if (levels.Count == 0)
        {
            throw new ArgumentException("A table needs at least one level.", nameof(levels));
        }

        if (levels.Select(l => l.PercentOfAmi).Distinct().Count() != levels.Count)
        {
            throw new ArgumentException("No two levels can have the same percent.", nameof(levels));
        }

        return new IncomeLimitTable(effectiveFrom, [.. levels.OrderBy(l => l.PercentOfAmi)]);
    }

    /// <summary>Rebuilds a table from the levels an event stored.</summary>
    /// <param name="effectiveFrom">The date the schedule applies from.</param>
    /// <param name="entries">The stored levels.</param>
    /// <returns>The table.</returns>
    public static IncomeLimitTable FromEntries(DateOnly effectiveFrom, IEnumerable<IncomeLimitLevelEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        return Create(effectiveFrom, [.. entries.Select(e => IncomeLimitLevel.Create(e.PercentOfAmi, e.Limits))]);
    }

    /// <summary>Gets the levels in the form an event stores them, lowest percent first.</summary>
    /// <returns>The levels.</returns>
    public IReadOnlyList<IncomeLimitLevelEntry> ToEntries() =>
        [.. Levels.Select(l => new IncomeLimitLevelEntry(l.PercentOfAmi, l.Limits))];

    /// <summary>Gets the level for <paramref name="percentOfAmi"/>, or <see langword="null"/> when the table has none.</summary>
    /// <param name="percentOfAmi">The percent of area median income.</param>
    /// <returns>The level, if the table has it.</returns>
    public IncomeLimitLevel? LevelFor(int percentOfAmi) =>
        Levels.FirstOrDefault(l => l.PercentOfAmi == percentOfAmi);

    /// <summary>
    /// Gets the band an income falls in: the lowest level whose limit for <paramref name="householdSize"/> is at
    /// or above <paramref name="annualIncome"/>, or above the highest level when every limit is lower.
    /// </summary>
    /// <param name="annualIncome">The household's annual income.</param>
    /// <param name="householdSize">The number of people in the household, at least 1.</param>
    /// <returns>The band.</returns>
    public IncomeBand BandFor(Money annualIncome, int householdSize)
    {
        ArgumentNullException.ThrowIfNull(annualIncome);

        int? over = null;
        foreach (var level in Levels)
        {
            if (annualIncome <= level.LimitFor(householdSize))
            {
                return IncomeBand.Create(over, level.PercentOfAmi);
            }

            over = level.PercentOfAmi;
        }

        return IncomeBand.Create(over, null);
    }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return EffectiveFrom;
        foreach (var level in Levels)
        {
            yield return level;
        }
    }
}
