using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.IncomeLimitSchedules.Events;

/// <summary>Raised when staff enter a published income limit schedule.</summary>
/// <param name="AggregateId">The schedule's ID.</param>
/// <param name="EffectiveFrom">The date the schedule applies from.</param>
/// <param name="Levels">The schedule's levels, lowest percent first.</param>
/// <param name="EnteredById">The staff user who entered it.</param>
public sealed record IncomeLimitScheduleEntered(
    string AggregateId,
    DateOnly EffectiveFrom,
    IReadOnlyList<EnteredIncomeLimitLevel> Levels,
    string EnteredById)
    : DomainEvent(AggregateId);

/// <summary>One level of an entered or corrected schedule.</summary>
/// <param name="PercentOfAmi">The percent of area median income.</param>
/// <param name="Limits">The limit for 1 to 8 people, in that order.</param>
public sealed record EnteredIncomeLimitLevel(int PercentOfAmi, IReadOnlyList<Money> Limits);
