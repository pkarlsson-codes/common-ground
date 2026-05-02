using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

/// <summary>Raised when a new lease violation is identified.</summary>
/// <param name="AggregateId">The stewardship case identifier.</param>
/// <param name="ViolationId">The new violation identifier.</param>
/// <param name="ViolationType">The category of violation.</param>
/// <param name="Description">Description of the violation.</param>
/// <param name="IdentifiedDate">The date the violation was identified.</param>
/// <param name="IdentifiedBy">The staff member who identified the violation.</param>
public sealed record LeaseViolationIdentified(
    string AggregateId,
    Guid ViolationId,
    LeaseViolationType ViolationType,
    string Description,
    DateOnly IdentifiedDate,
    string IdentifiedBy) : DomainEvent(AggregateId);
