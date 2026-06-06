using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.ApplicantSelection.Domain.HomeownerApplications.Events;

/// <summary>Application submitted event.</summary>
public sealed record ApplicationSubmitted(
    string AggregateId,
    string ApplicantFullName,
    string ApplicantEmail,
    string? ApplicantPhone,
    int HouseholdSize,
    decimal AnnualIncome,
    bool AccessibilityRequired,
    string SubmittedBy) : DomainEvent(AggregateId);
