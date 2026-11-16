using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Identity.Domain.Users.Events;

public sealed record UserTrustAssigned(
    string AggregateId,
    string TrustId)
    : DomainEvent(AggregateId);
