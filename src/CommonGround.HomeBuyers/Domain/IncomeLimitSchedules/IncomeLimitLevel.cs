using CommonGround.HomeBuyers.Application.Errors;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.IncomeLimitSchedules;

/// <summary>
/// One row of a published income limit table: the most a household may earn at a percent of area median
/// income (AMI), for each household size from 1 to 8 people.
/// </summary>
public sealed class IncomeLimitLevel : ValueObject
{
    /// <summary>The household sizes a published table prints a limit for: 1 to this many people.</summary>
    public const int PrintedHouseholdSizes = 8;

    /// <summary>The lowest percent of AMI a level can be.</summary>
    public const int MinimumPercentOfAmi = 1;

    /// <summary>The highest percent of AMI a level can be.</summary>
    public const int MaximumPercentOfAmi = 200;

    private const int FourPersonSize = 4;
    private const decimal ExtraPersonShareOfFourPersonLimit = 0.08m;
    private const decimal RoundingStep = 50m;

    private IncomeLimitLevel(int percentOfAmi, IReadOnlyList<Money> limits)
    {
        PercentOfAmi = percentOfAmi;
        Limits = limits;
    }

    /// <summary>Gets the percent of AMI, such as 80.</summary>
    public int PercentOfAmi { get; }

    /// <summary>Gets the published limits; the first is for 1 person, the last for 8.</summary>
    public IReadOnlyList<Money> Limits { get; }

    /// <summary>Creates a level from the published amounts.</summary>
    /// <param name="percentOfAmi">The percent of AMI, from 1 to 200.</param>
    /// <param name="limits">The limit for 1 to 8 people, in that order, each above zero.</param>
    /// <returns>The level.</returns>
    public static IncomeLimitLevel Create(int percentOfAmi, IReadOnlyList<Money> limits)
    {
        ArgumentNullException.ThrowIfNull(limits);

        if (percentOfAmi is < MinimumPercentOfAmi or > MaximumPercentOfAmi)
        {
            throw BusinessRuleViolation.IncomeLimitSchedule.Level.PercentOutOfRange(percentOfAmi);
        }

        if (limits.Count != PrintedHouseholdSizes)
        {
            throw BusinessRuleViolation.IncomeLimitSchedule.Level.WrongNumberOfLimits(percentOfAmi, limits.Count);
        }

        for (var i = 0; i < limits.Count; i++)
        {
            if (!limits[i].IsPositive)
            {
                throw BusinessRuleViolation.IncomeLimitSchedule.Level.LimitNotPositive(percentOfAmi, householdSize: i + 1);
            }
        }

        return new IncomeLimitLevel(percentOfAmi, [.. limits]);
    }

    /// <summary>
    /// Gets the limit for a household of <paramref name="householdSize"/> people. Above 8 people it is the
    /// 8-person limit plus 8% of the 4-person limit for each person over 8, rounded to the nearest $50,
    /// as HUD does for sizes its tables do not print. An amount exactly halfway rounds down, so the limit
    /// never allows a household the funder would refuse.
    /// </summary>
    /// <param name="householdSize">The number of people in the household, at least 1.</param>
    /// <returns>The limit.</returns>
    public Money LimitFor(int householdSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(householdSize, 1);

        if (householdSize <= PrintedHouseholdSizes)
        {
            return Limits[householdSize - 1];
        }

        var extraPeople = householdSize - PrintedHouseholdSizes;
        var limit = Limits[PrintedHouseholdSizes - 1].Amount
            + (ExtraPersonShareOfFourPersonLimit * Limits[FourPersonSize - 1].Amount * extraPeople);

        var steps = limit / RoundingStep;
        var lowerSteps = decimal.Floor(steps);
        var roundedSteps = steps - lowerSteps > 0.5m ? lowerSteps + 1 : lowerSteps;
        return Money.Create(roundedSteps * RoundingStep);
    }

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PercentOfAmi;
        foreach (var limit in Limits)
        {
            yield return limit;
        }
    }
}
