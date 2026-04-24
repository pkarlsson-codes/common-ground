using CommonGround.Identity.Domain.Users;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;
using Marten;
using Microsoft.Extensions.Logging;

namespace CommonGround.Identity.Infrastructure.Persistence;

internal sealed class UserRepository(
    IDocumentSession session,
    ILogger<UserRepository> logger)
    : MartenRepository<User, UserId>(session, logger),
    IUserRepository
{
    /// <inheritdoc/>
    public Task<UserSubjectIndex?> LoadSubjectIndexAsync(string keycloakSubject, CancellationToken cancellationToken = default)
        => Session.LoadAsync<UserSubjectIndex>(keycloakSubject, cancellationToken);

    /// <inheritdoc/>
    public void StoreSubjectIndex(UserSubjectIndex index) => Session.Store(index);
}
