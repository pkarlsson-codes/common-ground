using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

public sealed record ViolationNoticeIssued(
    string AggregateId,
    Guid ViolationId,
    DateOnly NoticeDate,
    DateOnly CureDeadline,
    string IssuedBy) : DomainEvent(AggregateId);
