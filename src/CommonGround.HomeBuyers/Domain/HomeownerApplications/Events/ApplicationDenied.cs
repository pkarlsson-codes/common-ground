using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;

/// <summary>Application denied event.</summary>
public sealed record ApplicationDenied(
    string AggregateId,
    string Reason,
    DateOnly DeniedDate,
    string RecordedBy) : DomainEvent(AggregateId);
