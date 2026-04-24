using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

/// <summary>Raised when a lease violation is escalated for further action.</summary>
/// <param name="AggregateId">The stewardship case identifier.</param>
/// <param name="ViolationId">The escalated violation identifier.</param>
/// <param name="Reason">The reason for escalation.</param>
/// <param name="EscalatedDate">The date of escalation.</param>
/// <param name="EscalatedBy">The staff member who escalated the violation.</param>
/// <param name="OccurredAt">When the event occurred.</param>
public sealed record LeaseViolationEscalated(
    string AggregateId,
    Guid ViolationId,
    string Reason,
    DateOnly EscalatedDate,
    string EscalatedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
