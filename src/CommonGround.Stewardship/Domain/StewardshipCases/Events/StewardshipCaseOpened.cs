using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

/// <summary>Raised when a new stewardship case is opened for a ground lease.</summary>
/// <param name="AggregateId">The stewardship case identifier.</param>
/// <param name="GroundLeaseId">The ground lease this case tracks.</param>
/// <param name="OpenedDate">The date the case was opened.</param>
/// <param name="OpenedBy">The user who opened the case.</param>
public sealed record StewardshipCaseOpened(
    string AggregateId,
    string GroundLeaseId,
    DateOnly OpenedDate,
    string OpenedBy) : DomainEvent(AggregateId);
