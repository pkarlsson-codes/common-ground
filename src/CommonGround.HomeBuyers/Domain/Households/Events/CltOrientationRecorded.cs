using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.Households.Events;

/// <summary>CLT orientation recorded event.</summary>
public sealed record CltOrientationRecorded(
    string AggregateId,
    DateOnly CompletedDate,
    string RecordedBy) : DomainEvent(AggregateId);
