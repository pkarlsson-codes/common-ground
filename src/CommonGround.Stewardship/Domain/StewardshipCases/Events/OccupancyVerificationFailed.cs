using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

public sealed record OccupancyVerificationFailed(
    string AggregateId,
    Guid VerificationId,
    string Reason,
    DateOnly FailedDate,
    string RecordedBy) : DomainEvent(AggregateId);
