using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

public sealed record StewardshipCheckInRecorded(
    string AggregateId,
    DateOnly CheckInDate,
    string Notes,
    string RecordedBy) : DomainEvent(AggregateId);
