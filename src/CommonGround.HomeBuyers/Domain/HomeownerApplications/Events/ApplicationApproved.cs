using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;

/// <summary>Eligibility approved event.</summary>
public sealed record ApplicationApproved(
    string AggregateId,
    DateOnly ApprovedDate,
    string? Notes,
    string RecordedBy) : DomainEvent(AggregateId);
