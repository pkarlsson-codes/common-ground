using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

/// <summary>Raised when a stewardship case is closed.</summary>
/// <param name="AggregateId">The stewardship case identifier.</param>
/// <param name="Reason">The reason for closing the case.</param>
/// <param name="ClosedDate">The date the case was closed.</param>
/// <param name="ClosedBy">The staff member who closed the case.</param>
/// <param name="OccurredAt">When the event occurred.</param>
public sealed record StewardshipCaseClosed(
    string AggregateId,
    string Reason,
    DateOnly ClosedDate,
    string ClosedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
