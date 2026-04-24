using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;

/// <summary>Raised when a CLT cure period is activated under a lender recognition agreement.</summary>
/// <param name="AggregateId">The foreclosure prevention case identifier.</param>
/// <param name="ActivationId">The new cure period activation identifier.</param>
/// <param name="LraId">Reference to the lender recognition agreement.</param>
/// <param name="ActivationDate">The date the cure period was activated.</param>
/// <param name="CurePeriodDays">The number of days in the cure period.</param>
/// <param name="CureDeadline">The deadline by which the delinquency must be cured.</param>
/// <param name="ActivatedBy">The staff member who activated the cure period.</param>
/// <param name="OccurredAt">When the event occurred.</param>
public sealed record CurePeriodActivated(
    string AggregateId,
    Guid ActivationId,
    string LraId,
    DateOnly ActivationDate,
    int CurePeriodDays,
    DateOnly CureDeadline,
    string ActivatedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
