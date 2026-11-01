using CommonGround.Identity.Application.ReadModels;
using CommonGround.Identity.Domain.Users;
using CommonGround.SharedKernel.Domain;
using Marten;
using Wolverine.Marten;

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
    /// <summary>Finds the user already registered for the subject, if any.</summary>
    public static async Task<UserView?> LoadAsync(
        EnsureUser command,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Query by explicit KeycloakSubject field rather than relying on projection document id.
        var existingBySubject = await session.Query<UserView>()
            .Where(x => x.KeycloakSubject == command.KeycloakSubject)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        // Backward compatibility fallback for existing rows keyed directly by subject.
        return existingBySubject
            ?? await session.LoadAsync<UserView>(command.KeycloakSubject, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Returns the existing user, or registers a new one without a trust.</summary>
    public static (EnsureUserResult, IMartenOp) Handle(EnsureUser command, UserView? existing)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (existing is not null)
        {
            return (new EnsureUserResult(existing.UserId, existing.TrustId), MartenOps.Nothing());
        }

        var userId = UserId.Generate();
        var registered = User.Register(userId, command.KeycloakSubject, command.Email, command.DisplayName);

        return (new EnsureUserResult(userId.Value, null), MartenOps.StartStream<User>(userId.Value, registered));
    }
}
