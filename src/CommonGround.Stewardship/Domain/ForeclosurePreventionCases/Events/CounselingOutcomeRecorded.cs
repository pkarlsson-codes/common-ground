using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;

/// <summary>Raised when the outcome of a counseling referral is recorded.</summary>
/// <param name="AggregateId">The foreclosure prevention case identifier.</param>
/// <param name="ReferralId">The referral for which the outcome is recorded.</param>
/// <param name="Outcome">Description of the counseling outcome.</param>
/// <param name="OutcomeDate">The date the outcome was recorded.</param>
/// <param name="RecordedBy">The staff member who recorded the outcome.</param>
/// <param name="OccurredAt">When the event occurred.</param>
public sealed record CounselingOutcomeRecorded(
    string AggregateId,
    Guid ReferralId,
    string Outcome,
    DateOnly OutcomeDate,
    string RecordedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
