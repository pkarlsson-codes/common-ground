using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.Households.Events;

public sealed record CltOrientationRecorded(
    string AggregateId,
    DateOnly CompletedDate,
    string RecordedBy) : DomainEvent(AggregateId);
