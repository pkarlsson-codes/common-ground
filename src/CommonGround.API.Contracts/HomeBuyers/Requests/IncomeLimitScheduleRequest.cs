namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>Request to enter an income limit schedule, or to correct one.</summary>
/// <param name="EffectiveFrom">The date the schedule applies from.</param>
/// <param name="Levels">The schedule's levels.</param>
public sealed record IncomeLimitScheduleRequest(
    DateOnly EffectiveFrom,
    IReadOnlyList<IncomeLimitLevelRequest> Levels);

/// <summary>One level of an income limit schedule, as published.</summary>
/// <param name="PercentOfAmi">The percent of area median income, from 1 to 200.</param>
/// <param name="Limits">The limit for 1 to 8 people, in that order.</param>
public sealed record IncomeLimitLevelRequest(
    int PercentOfAmi,
    IReadOnlyList<decimal> Limits);
