using CommonGround.Identity.Application.ReadModels;
using CommonGround.Identity.Domain.Users;
using CommonGround.SharedKernel.Domain;
using Marten;

namespace CommonGround.Identity.Application.Commands;

/// <summary>Idempotent upsert: returns the existing UserId if the subject is known, otherwise creates a new User.</summary>
/// <param name="KeycloakSubject">The Keycloak sub claim.</param>
/// <param name="Email">Optional email address.</param>
/// <param name="DisplayName">Optional display name.</param>
public sealed record EnsureUser(
    string KeycloakSubject,
    string? Email,
    string? DisplayName);

/// <summary>Result of <see cref="EnsureUser"/>.</summary>
/// <param name="UserId">The CommonGround UserId.</param>
/// <param name="TrustId">The trust this user belongs to, if assigned.</param>
public sealed record EnsureUserResult(string UserId, string? TrustId);

/// <summary>Handles <see cref="EnsureUser"/>.</summary>
public static class EnsureUserHandler
{
    /// <summary>Returns the existing or newly-created UserId and TrustId for the given Keycloak subject.</summary>
    public static async Task<EnsureUserResult> Handle(
        EnsureUser command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Query by explicit KeycloakSubject field rather than relying on projection document id.
        var existingBySubject = await session.Query<UserView>()
            .Where(x => x.KeycloakSubject == command.KeycloakSubject)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existingBySubject is not null)
        {
            return new EnsureUserResult(existingBySubject.UserId, existingBySubject.TrustId);
        }

        // Backward compatibility fallback for existing rows keyed directly by subject.
        var existing = await session
            .LoadAsync<UserView>(command.KeycloakSubject, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            return new EnsureUserResult(existing.UserId, existing.TrustId);
        }

        // Create a new user without an initial trust assignment
        var newUser = User.Register(command.KeycloakSubject, command.Email, command.DisplayName);

        session.Events.StartStream<User>(newUser.Id.Value, newUser.UncommittedEvents.ToArray());
        newUser.UncommittedEvents.MarkAsCommitted();

        return new EnsureUserResult(newUser.Id.Value, null);
    }
}

