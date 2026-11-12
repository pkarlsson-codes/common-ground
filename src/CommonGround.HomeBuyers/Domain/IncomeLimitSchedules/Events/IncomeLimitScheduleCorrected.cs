using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.IncomeLimitSchedules.Events;

/// <summary>Raised when staff correct a schedule's effective date or amounts. It replaces the whole schedule.</summary>
/// <param name="AggregateId">The schedule's ID.</param>
/// <param name="EffectiveFrom">The corrected date the schedule applies from.</param>
/// <param name="Levels">The corrected levels, lowest percent first.</param>
/// <param name="CorrectedById">The staff user who corrected it.</param>
public sealed record IncomeLimitScheduleCorrected(
    string AggregateId,
    DateOnly EffectiveFrom,
    IReadOnlyList<EnteredIncomeLimitLevel> Levels,
    string CorrectedById)
    : DomainEvent(AggregateId);
