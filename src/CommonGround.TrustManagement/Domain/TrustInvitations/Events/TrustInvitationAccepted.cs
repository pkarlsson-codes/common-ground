using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.TrustManagement.Domain.TrustInvitations.Events;

/// <summary>Raised when a user joins a trust with an invitation.</summary>
public sealed record TrustInvitationAccepted(
    string AggregateId,
    string TrustId,
    string UserId) : DomainEvent(AggregateId);
