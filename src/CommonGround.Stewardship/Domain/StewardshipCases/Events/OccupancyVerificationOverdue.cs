using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

/// <summary>Raised when an occupancy verification is marked overdue after the deadline has passed.</summary>
/// <param name="AggregateId">The stewardship case identifier.</param>
/// <param name="VerificationId">The overdue verification.</param>
/// <param name="Deadline">The deadline that was missed.</param>
/// <param name="OccurredAt">When the event occurred.</param>
public sealed record OccupancyVerificationOverdue(
    string AggregateId,
    Guid VerificationId,
    DateOnly Deadline,
    DateTimeOffset OccurredAt) : IDomainEvent;
