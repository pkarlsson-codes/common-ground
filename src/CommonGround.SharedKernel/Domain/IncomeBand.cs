namespace CommonGround.SharedKernel.Domain;

/// <summary>
/// Which of a schedule's levels a household's income falls in: over one level's limit and at or under the next
/// level's. The lowest band has no lower level ("up to 30% of AMI"); the band above the highest level has no
/// upper one ("over 120% of AMI").
/// </summary>
public sealed class IncomeBand : ValueObject
{
    private IncomeBand(int? overPercentOfAmi, int? upToPercentOfAmi)
    {
        OverPercentOfAmi = overPercentOfAmi;
        UpToPercentOfAmi = upToPercentOfAmi;
    }

    /// <summary>Gets the percent of the level whose limit the income is over, or null for the lowest band.</summary>
    public int? OverPercentOfAmi { get; }

    /// <summary>Gets the percent of the level whose limit the income is at or under, or null above the highest level.</summary>
    public int? UpToPercentOfAmi { get; }

    /// <summary>Creates a band from its bounds.</summary>
    /// <param name="overPercentOfAmi">The level below the band, or null for the lowest band.</param>
    /// <param name="upToPercentOfAmi">The level the band ends at, or null above the highest level.</param>
    /// <returns>The band.</returns>
    public static IncomeBand Create(int? overPercentOfAmi, int? upToPercentOfAmi)
    {
        if (overPercentOfAmi is null && upToPercentOfAmi is null)
        {
            throw new ArgumentException("A band needs at least one bound.", nameof(upToPercentOfAmi));
        }

        if (overPercentOfAmi is { } over && upToPercentOfAmi is { } upTo && over >= upTo)
        {
            throw new ArgumentException("A band's lower level must be below its upper level.", nameof(upToPercentOfAmi));
        }

        return new IncomeBand(overPercentOfAmi, upToPercentOfAmi);
    }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return OverPercentOfAmi ?? 0;
        yield return UpToPercentOfAmi ?? 0;
    }
}
