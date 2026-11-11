using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Identity.Domain.Users.Events;

/// <summary>Raised when a user signs in with an email address or name different from the ones on record.</summary>
public sealed record UserProfileUpdated(
    string AggregateId,
    string? Email,
    string? DisplayName) : DomainEvent(AggregateId);
