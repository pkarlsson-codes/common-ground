using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.TrustManagement.Domain.TrustInvitations.Events;

/// <summary>Raised when a trust member cancels an invitation before it is used.</summary>
/// <param name="AggregateId">The invitation id.</param>
/// <param name="TrustId">The invitation's trust.</param>
/// <param name="CancelledBy">The member who cancelled it.</param>
public sealed record TrustInvitationCancelled(
    string AggregateId,
    string TrustId,
    string CancelledBy) : DomainEvent(AggregateId);
