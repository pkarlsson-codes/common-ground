using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Identity.Domain.Users.Events;

public sealed record UserRegistered(
    string AggregateId,
    string KeycloakSubject,
    string? Email,
    string? DisplayName) : DomainEvent(AggregateId);
