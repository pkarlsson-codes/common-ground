using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.ApplicantSelection.Domain.HomeownerApplications.Events;

/// <summary>Eligibility approved event.</summary>
public sealed record EligibilityApproved(
    string AggregateId,
    DateOnly ApprovedDate,
    string? Notes,
    string RecordedBy) : DomainEvent(AggregateId);
