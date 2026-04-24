using CommonGround.Identity.Infrastructure.Persistence;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.Identity.Domain.Users;

/// <summary>Repository abstraction for <see cref="User"/> aggregates.</summary>
public interface IUserRepository : IRepository<User, UserId>
{
    /// <summary>Loads the <see cref="UserSubjectIndex"/> for the given Keycloak subject, or null if not found.</summary>
    Task<UserSubjectIndex?> LoadSubjectIndexAsync(string keycloakSubject, CancellationToken cancellationToken = default);

    /// <summary>Queues a <see cref="UserSubjectIndex"/> document for storage in the current session.</summary>
    void StoreSubjectIndex(UserSubjectIndex index);
}
