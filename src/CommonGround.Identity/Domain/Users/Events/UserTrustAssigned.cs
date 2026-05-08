using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Identity.Domain.Users.Events;

/// <summary>Raised when a user is assigned to a trust.</summary>
public sealed record UserTrustAssigned(
    string AggregateId,
    string TrustId)
    : DomainEvent(AggregateId);
