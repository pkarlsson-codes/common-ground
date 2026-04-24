using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

/// <summary>Raised when a lease violation is cured by the homeowner.</summary>
/// <param name="AggregateId">The stewardship case identifier.</param>
/// <param name="ViolationId">The cured violation identifier.</param>
/// <param name="CuredDate">The date the cure was verified.</param>
/// <param name="VerifiedBy">The staff member who verified the cure.</param>
/// <param name="OccurredAt">When the event occurred.</param>
public sealed record LeaseViolationCured(
    string AggregateId,
    Guid ViolationId,
    DateOnly CuredDate,
    string VerifiedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
