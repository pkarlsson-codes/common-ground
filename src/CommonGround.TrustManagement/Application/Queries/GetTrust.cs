using CommonGround.TrustManagement.Domain.Trusts;
using Marten;

namespace CommonGround.TrustManagement.Application.Queries;

/// <summary>Gets a trust by its id.</summary>
/// <param name="TrustId">The trust (tenant) identifier.</param>
public sealed record GetTrust(string TrustId);

/// <summary>Handles <see cref="GetTrust"/>.</summary>
public static class GetTrustHandler
{
    /// <summary>Rebuilds the trust from its stream, or returns null when it does not exist.</summary>
    public static async Task<Trust?> Handle(
        GetTrust query,
        IDocumentStore store,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Trust streams are not tenanted: they are written from the default-tenant session.
        var session = store.QuerySession();
        await using var _ = session.ConfigureAwait(false);
        return await session.Events.AggregateStreamAsync<Trust>(query.TrustId, token: cancellationToken).ConfigureAwait(false);
    }
}
