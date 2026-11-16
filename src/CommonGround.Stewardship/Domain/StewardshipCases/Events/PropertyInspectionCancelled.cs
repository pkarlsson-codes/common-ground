using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

public sealed record PropertyInspectionCancelled(
    string AggregateId,
    Guid InspectionId,
    string Reason,
    string CancelledBy) : DomainEvent(AggregateId);
