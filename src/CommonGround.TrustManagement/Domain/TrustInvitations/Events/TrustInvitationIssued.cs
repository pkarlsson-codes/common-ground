using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.TrustManagement.Domain.TrustInvitations.Events;

/// <summary>Raised when a trust member makes an invite link for a colleague.</summary>
/// <param name="AggregateId">The invitation id.</param>
/// <param name="TrustId">The trust the invitation joins.</param>
/// <param name="Email">The address the invitation is for.</param>
/// <param name="TokenHash">SHA-256 of the link's secret token, as lowercase hex.</param>
/// <param name="IssuedBy">The member who made it.</param>
/// <param name="ExpiresAt">When the link stops working.</param>
public sealed record TrustInvitationIssued(
    string AggregateId,
    string TrustId,
    string Email,
    string TokenHash,
    string IssuedBy,
    DateTimeOffset ExpiresAt) : DomainEvent(AggregateId);
