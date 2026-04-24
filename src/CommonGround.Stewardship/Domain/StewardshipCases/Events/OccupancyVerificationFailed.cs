using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

/// <summary>Raised when an occupancy verification is failed.</summary>
/// <param name="AggregateId">The stewardship case identifier.</param>
/// <param name="VerificationId">The verification that failed.</param>
/// <param name="Reason">The reason for failure.</param>
/// <param name="FailedDate">The date the failure was recorded.</param>
/// <param name="RecordedBy">The staff member who recorded the failure.</param>
/// <param name="OccurredAt">When the event occurred.</param>
public sealed record OccupancyVerificationFailed(
    string AggregateId,
    Guid VerificationId,
    string Reason,
    DateOnly FailedDate,
    string RecordedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
