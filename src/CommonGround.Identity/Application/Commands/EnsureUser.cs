using CommonGround.Identity.Domain.Users;
using CommonGround.Identity.Infrastructure.Persistence;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Identity.Application.Commands;

/// <summary>Idempotent upsert: returns the existing UserId if the subject is known, otherwise creates a new User.</summary>
/// <param name="KeycloakSubject">The Keycloak sub claim.</param>
/// <param name="Email">Optional email address.</param>
/// <param name="DisplayName">Optional display name.</param>
/// <param name="TrustId">The trust the user belongs to.</param>
public sealed record EnsureUser(
    string KeycloakSubject,
    string? Email,
    string? DisplayName,
    TrustId TrustId);

/// <summary>Result of <see cref="EnsureUser"/>.</summary>
public sealed record EnsureUserResult(string UserId, string TrustId);

/// <summary>Handles <see cref="EnsureUser"/>.</summary>
public static class EnsureUserHandler
{
    /// <summary>Returns the existing or newly-created UserId and TrustId for the given Keycloak subject.</summary>
    public static async Task<EnsureUserResult> Handle(
        EnsureUser command,
        IUserRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var existing = await repository
            .LoadSubjectIndexAsync(command.KeycloakSubject, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            var user = await repository
                .GetByIdAsync(UserId.Create(existing.UserId), cancellationToken)
                .ConfigureAwait(false);

            return new EnsureUserResult(existing.UserId, user!.TrustId.Value);
        }

        var newUser = User.Register(command.KeycloakSubject, command.Email, command.DisplayName, command.TrustId);

        repository.Stage(newUser);
        repository.StoreSubjectIndex(new UserSubjectIndex
        {
            Id = command.KeycloakSubject,
            UserId = newUser.Id.Value,
        });
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new EnsureUserResult(newUser.Id.Value, newUser.TrustId.Value);
    }
}
