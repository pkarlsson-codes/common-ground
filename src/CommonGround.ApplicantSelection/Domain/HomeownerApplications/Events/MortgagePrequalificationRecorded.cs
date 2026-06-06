using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.ApplicantSelection.Domain.HomeownerApplications.Events;

/// <summary>Mortgage prequalification recorded event.</summary>
public sealed record MortgagePrequalificationRecorded(
    string AggregateId,
    string LenderName,
    decimal ApprovedAmount,
    DateOnly ExpiryDate,
    string RecordedBy) : DomainEvent(AggregateId);
