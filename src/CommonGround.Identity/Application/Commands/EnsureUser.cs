using CommonGround.Identity.Application.ReadModels;
using CommonGround.Identity.Domain.Users;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using JasperFx.Events;
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

/// <summary>What <see cref="EnsureUser"/> decides from.</summary>
/// <param name="Existing">The user already registered for the subject, if any.</param>
/// <param name="Stream">That user's stream, fetched for writing, when there is one.</param>
public sealed record EnsureUserState(UserView? Existing, IEventStream<User>? Stream);

/// <summary>Handles <see cref="EnsureUser"/>.</summary>
public static class EnsureUserHandler
{
    /// <summary>Finds the user already registered for the subject, if any, and fetches their stream.</summary>
    public static async Task<EnsureUserState> LoadAsync(
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

        // Backward compatibility fallback for existing rows keyed directly by subject.
        var existing = existingBySubject
            ?? await session.LoadAsync<UserView>(command.KeycloakSubject, cancellationToken).ConfigureAwait(false);
        if (existing is null)
        {
            return new EnsureUserState(null, null);
        }

        var stream = await session.Events.FetchForWriting<User>(existing.UserId, cancellationToken).ConfigureAwait(false);
        return new EnsureUserState(existing, stream);
    }

    /// <summary>Returns the existing user, keeping the email and name they signed in with, or registers a new one without a trust.</summary>
    public static (EnsureUserResult, IMartenOp) Handle(EnsureUser command, EnsureUserState state)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(state);

        if (state.Existing is { } existing)
        {
            // A row kept from before users were event sourced has no stream to update.
            if (state.Stream is { Aggregate: not null } stream)
            {
                stream.Decide(u => u.UpdateProfile(command.Email, command.DisplayName));
            }

            return (new EnsureUserResult(existing.UserId, existing.TrustId), MartenOps.Nothing());
        }

        var userId = UserId.Generate();
        var registered = User.Register(userId, command.KeycloakSubject, command.Email, command.DisplayName);

        return (new EnsureUserResult(userId.Value, null), MartenOps.StartStream<User>(userId.Value, registered));
    }
}
