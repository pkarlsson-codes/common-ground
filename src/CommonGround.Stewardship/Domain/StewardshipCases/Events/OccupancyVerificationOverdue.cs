using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

/// <summary>Raised when an occupancy verification is marked overdue after the deadline has passed.</summary>
public sealed record OccupancyVerificationOverdue(
    string AggregateId,
    Guid VerificationId,
    DateOnly Deadline) : DomainEvent(AggregateId);
