using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;

/// <summary>Raised when a foreclosure prevention case is opened.</summary>
/// <param name="AggregateId">The foreclosure prevention case identifier.</param>
/// <param name="GroundLeaseId">The ground lease this case is associated with.</param>
/// <param name="Trigger">The event that triggered opening this case.</param>
/// <param name="OpenedDate">The date the case was opened.</param>
/// <param name="OpenedBy">The staff member who opened the case.</param>
public sealed record ForeclosurePreventionCaseOpened(
    string AggregateId,
    string GroundLeaseId,
    ForeclosurePreventionTrigger Trigger,
    DateOnly OpenedDate,
    string OpenedBy) : DomainEvent(AggregateId);
