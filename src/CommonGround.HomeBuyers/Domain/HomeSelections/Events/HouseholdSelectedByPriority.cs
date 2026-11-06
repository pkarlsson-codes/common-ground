using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeSelections.Events;

/// <summary>
/// Staff chose a household, citing the <see cref="SelectionPriority"/> values in
/// <paramref name="Priorities"/> and explaining the choice in <paramref name="Reason"/>.
/// </summary>
public sealed record HouseholdSelectedByPriority(
    string AggregateId,
    string ApplicationId,
    IReadOnlyList<SelectionPriority> Priorities,
    string Reason,
    string SelectedBy) : DomainEvent(AggregateId);
