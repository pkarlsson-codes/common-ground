using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeSelections.Events;

/// <summary>
/// Staff started choosing a household for a home. <paramref name="Candidates"/> is the waiting
/// list as it stood at that moment; later changes to an application don't alter it.
/// <paramref name="IncomeLimitPercentOfAmi"/> is the home's income limit the candidates were judged
/// against, null when it has none, and <paramref name="IncomeLimitsEffectiveFrom"/> the effective date
/// of the income limit schedule the amounts came from.
/// </summary>
public sealed record HomeSelectionStarted(
    string AggregateId,
    string HomeId,
    IReadOnlyList<SnapshotCandidate> Candidates,
    int? IncomeLimitPercentOfAmi,
    DateOnly? IncomeLimitsEffectiveFrom,
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
