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

    /// <summary>Gets the trust this user belongs to, if assigned.</summary>
    public TrustId? TrustId { get; private set; }

    /// <summary>Creates a new owned user record for the given Keycloak subject.</summary>
    public static User Register(string keycloakSubject, string? email, string? displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keycloakSubject);

        var user = new User();
        user.ApplyChange(new UserRegistered(
            UserId.Generate().Value,
            keycloakSubject,
            email,
            displayName));
        return user;
    }

    /// <summary>Assigns the user to a trust when no trust is currently assigned.</summary>
    public void AssignTrust(TrustId trustId)
    {

        if (TrustId is not null)
        {
            throw new InvalidOperationException("User already belongs to a trust.");
        }

        ApplyChange(new UserTrustAssigned(Id.Value, trustId.Value));
    }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case UserRegistered e: Apply(e); break;
            case UserTrustAssigned e: Apply(e); break;
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

    private void Apply(UserTrustAssigned e)
    {
        TrustId = global::CommonGround.SharedKernel.Domain.TrustId.Create(e.TrustId);
    }
}
