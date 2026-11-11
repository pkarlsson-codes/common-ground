using CommonGround.TrustManagement.Application.ReadModels;
using Marten;

namespace CommonGround.TrustManagement.Application.Queries;

/// <summary>Finds an invitation by its link's token hash, for the invite page.</summary>
/// <param name="TokenHash">The hash of the link's token.</param>
public sealed record FindTrustInvitation(string TokenHash);

/// <summary>Handles <see cref="FindTrustInvitation"/>.</summary>
public static class FindTrustInvitationHandler
{
    /// <summary>Returns the invitation, or <see langword="null"/> when no link has that token.</summary>
    public static async Task<TrustInvitationView?> Handle(
        FindTrustInvitation query,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        return await session.Query<TrustInvitationView>()
            .SingleOrDefaultAsync(x => x.TokenHash == query.TokenHash, cancellationToken)
            .ConfigureAwait(false);
    }
}
