using CommonGround.Identity.Application.ReadModels;
using CommonGround.SharedKernel.Domain;
using Marten;

namespace CommonGround.Identity.Application.Queries;

/// <summary>Lists the users who belong to a trust.</summary>
/// <param name="TrustId">The trust.</param>
public sealed record ListTrustMembers(TrustId TrustId);

/// <summary>Handles <see cref="ListTrustMembers"/>.</summary>
public static class ListTrustMembersHandler
{
    /// <summary>Returns the trust's users, by name then email.</summary>
    public static async Task<IReadOnlyList<UserView>> Handle(
        ListTrustMembers query,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var trustId = query.TrustId.Value;
        var members = await session.Query<UserView>()
            .Where(x => x.TrustId == trustId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return members
            .OrderBy(x => x.DisplayName ?? x.Email, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Email, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
