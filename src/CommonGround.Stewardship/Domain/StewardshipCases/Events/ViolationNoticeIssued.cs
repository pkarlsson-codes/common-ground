using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Stewardship.Domain.StewardshipCases.Events;

/// <summary>Raised when a formal violation notice is issued to the homeowner.</summary>
/// <param name="AggregateId">The stewardship case identifier.</param>
/// <param name="ViolationId">The violation for which the notice is issued.</param>
/// <param name="NoticeDate">The date the notice was issued.</param>
/// <param name="CureDeadline">The deadline for curing the violation.</param>
/// <param name="IssuedBy">The staff member who issued the notice.</param>
/// <param name="OccurredAt">When the event occurred.</param>
public sealed record ViolationNoticeIssued(
    string AggregateId,
    Guid ViolationId,
    DateOnly NoticeDate,
    DateOnly CureDeadline,
    string IssuedBy,
    DateTimeOffset OccurredAt) : IDomainEvent;
