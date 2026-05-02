using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;

/// <summary>Raised when a foreclosure prevention case is resolved.</summary>
/// <param name="AggregateId">The foreclosure prevention case identifier.</param>
/// <param name="Outcome">The outcome of the case.</param>
/// <param name="ResolvedDate">The date the case was resolved.</param>
/// <param name="ResolvedBy">The staff member who resolved the case.</param>
public sealed record ForeclosurePreventionCaseResolved(
    string AggregateId,
    ForeclosurePreventionOutcome Outcome,
    DateOnly ResolvedDate,
    string ResolvedBy) : DomainEvent(AggregateId);
