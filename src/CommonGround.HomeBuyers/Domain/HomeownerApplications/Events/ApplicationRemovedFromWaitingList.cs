using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;

/// <summary>The trust took an approved application off the waiting list.</summary>
/// <param name="AggregateId">The application.</param>
/// <param name="Reason">Why.</param>
/// <param name="Note">Staff's note; always set when the reason is Other.</param>
/// <param name="RemovedDate">The day the trust took the household off the list.</param>
/// <param name="RecordedBy">The staff user recording it.</param>
public sealed record ApplicationRemovedFromWaitingList(
    string AggregateId,
    WaitingListRemovalReason Reason,
    string? Note,
    DateOnly RemovedDate,
    string RecordedBy) : DomainEvent(AggregateId);
