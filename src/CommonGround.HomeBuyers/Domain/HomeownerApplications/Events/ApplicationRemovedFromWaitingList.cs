using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;

/// <summary>The trust took an approved application off the waiting list.</summary>
/// <param name="Note">Staff's note; always set when the reason is Other.</param>
public sealed record ApplicationRemovedFromWaitingList(
    string AggregateId,
    WaitingListRemovalReason Reason,
    string? Note,
    DateOnly RemovedDate,
    string RecordedBy) : DomainEvent(AggregateId);
