using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.ApplicantSelection.Domain.HomeownerApplications.Events;

/// <summary>Added to waiting list event.</summary>
public sealed record AddedToWaitingList(
    string AggregateId,
    DateOnly AddedDate) : DomainEvent(AggregateId);
