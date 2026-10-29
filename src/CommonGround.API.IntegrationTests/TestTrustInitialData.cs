using CommonGround.TrustManagement.Domain.Trusts;
using CommonGround.TrustManagement.Domain.Trusts.Events;
using Marten;
using Marten.Schema;

namespace CommonGround.API.IntegrationTests;

/// <summary>Creates the trust behind <see cref="ApiFactory.TestTrustId"/> so its time zone resolves.</summary>
internal sealed class TestTrustInitialData : IInitialData
{
    /// <summary>The time zone of the test trust.</summary>
    internal const string TimeZone = "America/Denver";

    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        await using var session = store.LightweightSession();
        if (await session.Events.FetchStreamStateAsync(ApiFactory.TestTrustId, cancellation) is not null)
        {
            return;
        }

        session.Events.StartStream<Trust>(
            ApiFactory.TestTrustId,
            new TrustCreated(ApiFactory.TestTrustId, "Test Trust", TimeZone));
        await session.SaveChangesAsync(cancellation);
    }
}
