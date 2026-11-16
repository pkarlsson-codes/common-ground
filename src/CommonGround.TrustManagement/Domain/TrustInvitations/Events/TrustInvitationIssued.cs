using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.TrustManagement.Domain.TrustInvitations.Events;

/// <summary>Raised when a trust member makes an invite link for a colleague. <c>TokenHash</c> is the SHA-256 of the link's secret token, as lowercase hex.</summary>
public sealed record TrustInvitationIssued(
    string AggregateId,
    string TrustId,
    string Email,
    string TokenHash,
    string IssuedBy,
    DateTimeOffset ExpiresAt) : DomainEvent(AggregateId);
