using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;

public sealed record ApplicationDenied(
    string AggregateId,
    string Reason,
    DateOnly DeniedDate,
    string RecordedBy) : DomainEvent(AggregateId);
