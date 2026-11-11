using CommonGround.Identity.Domain.Users.Events;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Identity.Domain.Users;

/// <summary>Aggregate representing a CommonGround-owned user identity.</summary>
public sealed class User : AggregateRoot<UserId>
{
#pragma warning disable CS8618 // State is set by Create from the stream's first event.
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

    /// <summary>Decides to register an owned user record for the given Keycloak subject.</summary>
    /// <returns>The event that starts the user's stream.</returns>
    public static UserRegistered Register(UserId id, string keycloakSubject, string? email, string? displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keycloakSubject);

        return new UserRegistered(id.Value, keycloakSubject, email, displayName);
    }

    /// <summary>Decides to assign the user to a trust, unless they already belong to one.</summary>
    /// <returns>The assignment, or <see langword="null"/> when the user already has a trust.</returns>
    public UserTrustAssigned? AssignTrustIfNone(TrustId trustId) =>
        TrustId is null ? new UserTrustAssigned(Id.Value, trustId.Value) : null;

    /// <summary>Decides to keep the email address and name the sign-in server gives on a later sign-in.</summary>
    /// <param name="email">The email address it gave, or <see langword="null"/> to keep the one on record.</param>
    /// <param name="displayName">The name it gave, or <see langword="null"/> to keep the one on record.</param>
    /// <returns>The update, or <see langword="null"/> when nothing changed.</returns>
    public UserProfileUpdated? UpdateProfile(string? email, string? displayName)
    {
        var newEmail = string.IsNullOrWhiteSpace(email) ? Email : email;
        var newName = string.IsNullOrWhiteSpace(displayName) ? DisplayName : displayName;

        return newEmail == Email && newName == DisplayName ? null : new UserProfileUpdated(Id.Value, newEmail, newName);
    }

    /// <summary>Starts a user from its first event.</summary>
    public static User Create(UserRegistered e)
    {
        ArgumentNullException.ThrowIfNull(e);

        return new User
        {
            Id = UserId.Create(e.AggregateId),
            KeycloakSubject = e.KeycloakSubject,
            Email = e.Email,
            DisplayName = e.DisplayName,
            CreatedAt = e.OccurredAt,
        };
    }

    /// <summary>Applies <see cref="UserTrustAssigned"/>.</summary>
    public void Apply(UserTrustAssigned e)
    {
        ArgumentNullException.ThrowIfNull(e);

        TrustId = global::CommonGround.SharedKernel.Domain.TrustId.Create(e.TrustId);
    }

    /// <summary>Applies <see cref="UserProfileUpdated"/>.</summary>
    public void Apply(UserProfileUpdated e)
    {
        ArgumentNullException.ThrowIfNull(e);

        Email = e.Email;
        DisplayName = e.DisplayName;
    }
}
