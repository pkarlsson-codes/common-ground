using Marten;
using Wolverine.Runtime;

namespace CommonGround.SharedKernel.Infrastructure.Seeding;

/// <summary>
/// Propagation is pending while any async shard (the Wolverine event subscription included) is
/// behind the event store, or Wolverine's inbox holds messages to handle. Scheduled messages, such
/// as a prequalification's expiry, don't count: they run later, on the real clock.
/// </summary>
public sealed class MartenWolverinePropagationProbe(IDocumentStore store, IWolverineRuntime runtime) : IPropagationProbe
{
    /// <inheritdoc/>
    public async Task<string?> FindPendingAsync(CancellationToken cancellationToken)
    {
        var statistics = await store.Advanced.FetchEventStoreStatistics(token: cancellationToken).ConfigureAwait(false);
        var progress = (await store.Advanced.AllProjectionProgress(token: cancellationToken).ConfigureAwait(false))
            .ToDictionary(s => s.ShardName, s => s.Sequence, StringComparer.Ordinal);

        // A shard with no progress row hasn't run yet, so it is behind too. AllShards, unlike
        // AllAsyncProjectionShardNames, includes subscriptions, so the Wolverine relay is counted.
        var behind = ((StoreOptions)store.Options).Projections.AllShards()
            .Select(shard => (Name: shard.Name.Identity, Sequence: progress.GetValueOrDefault(shard.Name.Identity)))
            .Where(shard => shard.Sequence < statistics.EventSequenceNumber)
            .Select(shard => $"{shard.Name} at {shard.Sequence} of {statistics.EventSequenceNumber}")
            .ToList();
        if (behind.Count > 0)
        {
            return $"shards behind: {string.Join(", ", behind)}";
        }

        var counts = await runtime.Storage.Admin.FetchCountsAsync().ConfigureAwait(false);
        return counts.Incoming > 0 ? $"{counts.Incoming} incoming message(s) in Wolverine's inbox" : null;
    }
}
