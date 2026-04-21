using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>Raised when a stewardship check-in visit is recorded.</summary>
public sealed record StewardshipCheckInRecorded(
    string AggregateId,
    DateOnly CheckInDate,
    string Notes,
    string RecordedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
