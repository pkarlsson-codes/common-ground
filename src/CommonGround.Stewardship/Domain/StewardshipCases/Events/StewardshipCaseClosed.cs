using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

public sealed record StewardshipCaseClosed(
    string AggregateId,
    string Reason,
    DateOnly ClosedDate,
    string ClosedBy) : DomainEvent(AggregateId);
