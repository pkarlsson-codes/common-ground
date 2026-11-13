using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeSelections.Events;

/// <summary>
/// Staff started choosing a household for a home. <paramref name="Candidates"/> is the waiting
/// list as it stood at that moment; later changes to an application don't alter it.
/// <paramref name="IncomeLimitPercentOfAmi"/> is the home's income limit the candidates were judged
/// against, null when it has none. <paramref name="IncomeLimitsEffectiveFrom"/> and <paramref name="IncomeLimitLevels"/>
/// are the income limit schedule in effect when the selection started, every level of it, so the purchase can judge
/// a household whose size changed and record its income band. Both are null when no schedule was in effect.
/// </summary>
public sealed record HomeSelectionStarted(
    string AggregateId,
    string HomeId,
    IReadOnlyList<SnapshotCandidate> Candidates,
    int? IncomeLimitPercentOfAmi,
    DateOnly? IncomeLimitsEffectiveFrom,
    IReadOnlyList<IncomeLimitLevelEntry>? IncomeLimitLevels,
    string StartedBy) : DomainEvent(AggregateId);

/// <summary>
/// One approved application on the waiting list when the selection started. <paramref name="IncomeLimit"/>
/// is the home's limit for the household's size, null when the home has none.
/// </summary>
public sealed record SnapshotCandidate(
    string ApplicationId,
    string HouseholdId,
    Money AnnualIncome,
    int HouseholdSize,
    bool RequiresAccessibility,
    DateOnly WaitingSince,
    Money? IncomeLimit);
