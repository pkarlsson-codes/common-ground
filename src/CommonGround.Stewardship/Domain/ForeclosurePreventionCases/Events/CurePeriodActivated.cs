using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases.Events;

/// <summary>Raised when a CLT cure period is activated under a lender recognition agreement.</summary>
public sealed record CurePeriodActivated(
    string AggregateId,
    Guid ActivationId,
    string LraId,
    DateOnly ActivationDate,
    int CurePeriodDays,
    DateOnly CureDeadline,
    string ActivatedBy) : DomainEvent(AggregateId);
