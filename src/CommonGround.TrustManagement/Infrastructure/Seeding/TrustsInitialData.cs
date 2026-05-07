using System.Text.Json;
using CommonGround.TrustManagement.Domain.Trusts;
using Marten;
using Marten.Schema;

namespace CommonGround.TrustManagement.Infrastructure.Seeding;

/// <summary>Seeds development trusts from JSON and persists deterministic seed-key mappings.</summary>
public sealed class TrustsInitialData : IInitialData
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <inheritdoc/>
    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        ArgumentNullException.ThrowIfNull(store);

        var entries = await LoadEntriesAsync(cancellation).ConfigureAwait(false);
        if (entries.Count == 0)
        {
            return;
        }

        using var session = store.LightweightSession();

        foreach (var entry in entries)
        {
            var lookupId = TrustSeedLookup.BuildId(entry.SeedKey);
            var lookup = await session.LoadAsync<TrustSeedLookup>(lookupId, cancellation).ConfigureAwait(false);
            if (lookup is not null)
            {
                var streamState = await session.Events.FetchStreamStateAsync(lookup.TrustId, cancellation).ConfigureAwait(false);
                if (streamState is not null)
                {
                    continue;
                }
            }

            var trust = Trust.Create(entry.Name);
            session.Events.StartStream<Trust>(trust.Id.Value, trust.UncommittedEvents.ToArray());
            trust.UncommittedEvents.MarkAsCommitted();

            session.Store(new TrustSeedLookup
            {
                Id = lookupId,
                SeedKey = entry.SeedKey,
                TrustId = trust.Id.Value,
                SeededAt = DateTimeOffset.UtcNow,
            });

            await session.SaveChangesAsync(cancellation).ConfigureAwait(false);
        }
    }

    private static async Task<IReadOnlyList<TrustSeedEntry>> LoadEntriesAsync(CancellationToken cancellation)
    {
        var assembly = typeof(TrustsInitialData).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith("trusts.seed.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new InvalidOperationException("Could not find embedded trust seed resource 'trusts.seed.json'.");
        }

        var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not load seed resource stream '{resourceName}'.");

        using (stream)
        {
            var entries = await JsonSerializer.DeserializeAsync<List<TrustSeedEntry>>(stream, JsonOptions, cancellation)
                .ConfigureAwait(false)
                ?? [];

            var duplicateSeedKey = entries
                .GroupBy(x => x.SeedKey, StringComparer.Ordinal)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateSeedKey is not null)
            {
                throw new InvalidOperationException($"Duplicate trust seed key '{duplicateSeedKey.Key}' in seed JSON.");
            }

            return entries;
        }
    }

    private sealed record TrustSeedEntry(string SeedKey, string Name);
}
