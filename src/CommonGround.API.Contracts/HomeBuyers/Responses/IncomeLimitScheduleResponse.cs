namespace CommonGround.API.Contracts.HomeBuyers.Responses;

/// <summary>One of the trust's income limit schedules.</summary>
/// <param name="Id">The schedule's ID.</param>
/// <param name="EffectiveFrom">The date it applies from.</param>
/// <param name="IsInEffect">Whether it is the schedule in effect on the trust's today.</param>
/// <param name="Levels">Its levels, lowest percent first.</param>
/// <param name="EnteredBy">The ID of the staff user who entered it.</param>
/// <param name="EnteredAt">When it was entered.</param>
/// <param name="CorrectedBy">The ID of the staff user who last corrected it, if anyone did.</param>
/// <param name="CorrectedAt">When it was last corrected, if it was.</param>
public sealed record IncomeLimitScheduleResponse(
    string Id,
    DateOnly EffectiveFrom,
    bool IsInEffect,
    IReadOnlyList<IncomeLimitLevelResponse> Levels,
    string EnteredBy,
    DateTimeOffset EnteredAt,
    string? CorrectedBy,
    DateTimeOffset? CorrectedAt);

/// <summary>One level of an income limit schedule.</summary>
/// <param name="PercentOfAmi">The percent of area median income.</param>
/// <param name="Limits">The limit for 1 to 8 people, in that order.</param>
public sealed record IncomeLimitLevelResponse(
    int PercentOfAmi,
    IReadOnlyList<decimal> Limits);

/// <summary>Response returned after entering an income limit schedule.</summary>
/// <param name="ScheduleId">The new schedule's ID.</param>
public sealed record EnterIncomeLimitScheduleResponse(string ScheduleId);
