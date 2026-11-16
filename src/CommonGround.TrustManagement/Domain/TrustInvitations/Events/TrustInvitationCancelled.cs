using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.TrustManagement.Domain.TrustInvitations.Events;

/// <summary>Raised when a trust member cancels an invitation before it is used.</summary>
public sealed record TrustInvitationCancelled(
    string AggregateId,
    string TrustId,
    string CancelledBy) : DomainEvent(AggregateId);
