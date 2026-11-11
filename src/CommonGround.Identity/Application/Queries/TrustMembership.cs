using CommonGround.Identity.Application.ReadModels;
using CommonGround.SharedKernel.Domain;
using Marten;

namespace CommonGround.Identity.Application.Queries;

/// <summary>Checks whether a user still belongs to a trust.</summary>
public static class TrustMembership
{
    /// <summary>
    /// Returns <see langword="false"/> when the user is on record and doesn't belong to the trust.
    /// A user id with no record passes: every real user id comes from <c>EnsureUser</c>, and only the gateway can sign one into a token.
    /// </summary>
    public static async Task<bool> AllowsAsync(IQuerySession session, string userId, string trustId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);

        var user = await session.Query<UserView>()
            .Where(x => x.UserId == userId)
            .Select(x => new { x.TrustId })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return user is null || user.TrustId == trustId;
    }
}
