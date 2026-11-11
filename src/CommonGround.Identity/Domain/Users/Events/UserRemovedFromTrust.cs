using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Identity.Domain.Users.Events;

/// <summary>Raised when a member of a trust removes this user from it.</summary>
public sealed record UserRemovedFromTrust(
    string AggregateId,
    string TrustId,
    string RemovedBy)
    : DomainEvent(AggregateId);
