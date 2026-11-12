using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeSelections.Events;

/// <summary>
/// Staff started choosing a household for a home. <paramref name="Candidates"/> is the waiting
/// list as it stood at that moment; later changes to an application don't alter it.
/// </summary>
public sealed record HomeSelectionStarted(
    string AggregateId,
    string HomeId,
    IReadOnlyList<SnapshotCandidate> Candidates,
    string StartedBy) : DomainEvent(AggregateId);

/// <summary>One approved application on the waiting list when the selection started.</summary>
public sealed record SnapshotCandidate(
    string ApplicationId,
    string HouseholdId,
    Money AnnualIncome,
    int HouseholdSize,
    bool RequiresAccessibility,
    DateOnly WaitingSince);
