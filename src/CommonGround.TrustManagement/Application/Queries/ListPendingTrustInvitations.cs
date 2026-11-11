using CommonGround.SharedKernel.Domain;
using CommonGround.TrustManagement.Application.ReadModels;
using Marten;

namespace CommonGround.TrustManagement.Application.Queries;

/// <summary>Lists a trust's invitations that can still be used.</summary>
/// <param name="TrustId">The trust.</param>
/// <param name="Now">The current instant; invitations expired by then are left out.</param>
public sealed record ListPendingTrustInvitations(TrustId TrustId, DateTimeOffset Now);

/// <summary>Handles <see cref="ListPendingTrustInvitations"/>.</summary>
public static class ListPendingTrustInvitationsHandler
{
    /// <summary>Returns the usable invitations, newest first.</summary>
    public static async Task<IReadOnlyList<TrustInvitationView>> Handle(
        ListPendingTrustInvitations query,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var trustId = query.TrustId.Value;
        return await session.Query<TrustInvitationView>()
            .Where(x => x.TrustId == trustId && x.Status == TrustInvitationView.Pending && x.ExpiresAt > query.Now)
            .OrderByDescending(x => x.IssuedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
