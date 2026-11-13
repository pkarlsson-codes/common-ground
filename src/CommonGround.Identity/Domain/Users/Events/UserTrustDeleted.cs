using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Identity.Domain.Users.Events;

/// <summary>Raised when the trust this user belonged to is deleted. The user no longer belongs to a trust.</summary>
public sealed record UserTrustDeleted(
    string AggregateId,
    string TrustId)
    : DomainEvent(AggregateId);
