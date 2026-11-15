using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.HeirTransfers;

/// <summary>
/// The income limit a home carries, with the schedule in effect on the day judged: what an income-qualified heir
/// must be under (Model Lease Art 10.3).
/// </summary>
/// <param name="PercentOfAmi">The home's limit as a percent of area median income.</param>
/// <param name="Schedule">The income limit schedule in effect.</param>
public sealed record HeirIncomeLimit(int PercentOfAmi, IncomeLimitTable Schedule)
{
    /// <summary>Gets the most a household of this size may earn, or null when the schedule has no level for the home's percent.</summary>
    /// <param name="householdSize">The household size, from 1 to 8 people.</param>
    public Money? LimitFor(int householdSize) => Schedule.LevelFor(PercentOfAmi)?.LimitFor(householdSize);
}
