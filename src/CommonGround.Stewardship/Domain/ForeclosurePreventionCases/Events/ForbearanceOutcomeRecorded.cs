using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;

/// <summary>Raised when the outcome of a forbearance request is recorded.</summary>
/// <param name="AggregateId">The foreclosure prevention case identifier.</param>
/// <param name="RequestId">The forbearance request for which the outcome is recorded.</param>
/// <param name="Outcome">The lender's decision.</param>
/// <param name="OutcomeDate">The date the outcome was recorded.</param>
/// <param name="Notes">Optional notes on the outcome.</param>
/// <param name="RecordedBy">The staff member who recorded the outcome.</param>
public sealed record ForbearanceOutcomeRecorded(
    string AggregateId,
    Guid RequestId,
    ForbearanceOutcome Outcome,
    DateOnly OutcomeDate,
    string? Notes,
    string RecordedBy) : DomainEvent(AggregateId);
