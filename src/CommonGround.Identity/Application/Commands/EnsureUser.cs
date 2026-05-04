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

/// <summary>Handles <see cref="EnsureUser"/>.</summary>
public static class EnsureUserHandler
{
    /// <summary>Returns the existing or newly-created UserId for the given Keycloak subject.</summary>
    public static async Task<UserId> Handle(
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
            return UserId.Create(existing.UserId);
        }

        var user = User.Register(command.KeycloakSubject, command.Email, command.DisplayName, command.TrustId);

        repository.Stage(user);
        repository.StoreSubjectIndex(new UserSubjectIndex
        {
            Id = command.KeycloakSubject,
            UserId = user.Id.Value,
        });
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return user.Id;
    }
}
