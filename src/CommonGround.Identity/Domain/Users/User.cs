using CommonGround.Identity.Domain.Users.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Identity.Domain.Users;

/// <summary>Aggregate representing a CommonGround-owned user identity.</summary>
public sealed class User : AggregateRoot<UserId>
{
#pragma warning disable CS8618
    private User() { }
#pragma warning restore CS8618

    /// <summary>Gets the Keycloak subject (external credential reference).</summary>
    public string KeycloakSubject { get; private set; }

    /// <summary>Gets the user's email address, if known.</summary>
    public string? Email { get; private set; }

    /// <summary>Gets the user's display name, if known.</summary>
    public string? DisplayName { get; private set; }

    /// <summary>Gets when this user record was created.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Creates a new owned user record for the given Keycloak subject.</summary>
    public static User Register(string keycloakSubject, string? email, string? displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keycloakSubject);

        var user = new User();
        user.ApplyChange(new UserRegistered(
            UserId.Generate().Value,
            keycloakSubject,
            email,
            displayName,
            DateTimeOffset.UtcNow));
        return user;
    }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case UserRegistered e: Apply(e); break;
            default: throw new InvalidOperationException($"Unknown event: {domainEvent?.GetType().Name}");
        }
    }

    private void Apply(UserRegistered e)
    {
        Id = UserId.Create(e.AggregateId);
        KeycloakSubject = e.KeycloakSubject;
        Email = e.Email;
        DisplayName = e.DisplayName;
        CreatedAt = e.OccurredAt;
    }
}
