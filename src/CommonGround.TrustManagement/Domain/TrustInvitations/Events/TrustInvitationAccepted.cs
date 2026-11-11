using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.TrustManagement.Domain.TrustInvitations.Events;

/// <summary>Raised when a user joins a trust with an invitation.</summary>
/// <param name="AggregateId">The invitation id.</param>
/// <param name="TrustId">The trust joined.</param>
/// <param name="UserId">The user who joined.</param>
public sealed record TrustInvitationAccepted(
    string AggregateId,
    string TrustId,
    string UserId) : DomainEvent(AggregateId);
