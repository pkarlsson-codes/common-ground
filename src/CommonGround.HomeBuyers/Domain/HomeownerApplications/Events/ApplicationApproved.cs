using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;

/// <summary>Homeowner application approved event.</summary>
public sealed record ApplicationApproved(
    string AggregateId,
    DateOnly ApprovedDate,
    string? Notes,
    string RecordedBy) : DomainEvent(AggregateId);
