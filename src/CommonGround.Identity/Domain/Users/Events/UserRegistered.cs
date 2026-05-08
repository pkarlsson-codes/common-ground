using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Identity.Domain.Users.Events;

/// <summary>Raised when a new CommonGround-owned user record is created.</summary>
public sealed record UserRegistered(
    string AggregateId,
    string KeycloakSubject,
    string? Email,
    string? DisplayName,
    string? TrustId) : DomainEvent(AggregateId);
