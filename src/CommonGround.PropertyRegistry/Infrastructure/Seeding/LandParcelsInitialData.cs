using System.Text.Json;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using Marten;
using Marten.Schema;

namespace CommonGround.PropertyRegistry.Infrastructure.Seeding;

/// <summary>
/// Seeds development land parcels from JSON and persists deterministic seed-key mappings.
/// </summary>
public sealed class LandParcelsInitialData : IInitialData
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly UserId SeederUserId = new("user-seeder-system");

    /// <inheritdoc/>
    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        ArgumentNullException.ThrowIfNull(store);

        var entries = await LoadEntriesAsync(cancellation).ConfigureAwait(false);
        if (entries.Count == 0)
        {
            return;
        }

        using var defaultSession = store.LightweightSession();

        foreach (var entry in entries)
        {
            var trustLookup = await defaultSession
                .LoadAsync<TrustSeedLookup>(TrustSeedLookup.BuildId(entry.TrustSeedKey), cancellation)
                .ConfigureAwait(false)
                ?? throw new InvalidOperationException(
                    $"Trust seed '{entry.TrustSeedKey}' not found for land parcel seed '{entry.SeedKey}'. Ensure TrustsInitialData runs first.");

            using var trustSession = store.LightweightSession(trustLookup.TrustId);

            var lookupId = LandParcelSeedLookup.BuildId(entry.SeedKey);
            var lookup = await trustSession.LoadAsync<LandParcelSeedLookup>(lookupId, cancellation).ConfigureAwait(false);
            if (lookup is not null)
            {
                var streamState = await trustSession.Events.FetchStreamStateAsync(lookup.LandParcelId, cancellation).ConfigureAwait(false);
                if (streamState is not null)
                {
                    continue;
                }
            }

            var parcel = LandParcel.Register(
                entry.LegalParcelNumber,
                entry.Address,
                entry.AreaAcres,
                entry.AcquisitionDate,
                AcquisitionMethod.FromDisplayName(entry.AcquisitionMethod),
                entry.CovenantTerms,
                SeederUserId);

            trustSession.Events.StartStream<LandParcel>(parcel.Id.Value, parcel.UncommittedEvents.ToArray());
            parcel.UncommittedEvents.MarkAsCommitted();

            trustSession.Store(new LandParcelSeedLookup
            {
                Id = lookupId,
                SeedKey = entry.SeedKey,
                LandParcelId = parcel.Id.Value,
                SeededAt = DateTimeOffset.UtcNow,
            });

            await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
        }
    }

    private static async Task<IReadOnlyList<LandParcelSeedEntry>> LoadEntriesAsync(CancellationToken cancellation)
    {
        var assembly = typeof(LandParcelsInitialData).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith("land-parcels.seed.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new InvalidOperationException("Could not find embedded land parcel seed resource 'land-parcels.seed.json'.");
        }

        var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not load seed resource stream '{resourceName}'.");

        using (stream)
        {
            var entries = await JsonSerializer.DeserializeAsync<List<LandParcelSeedEntry>>(stream, JsonOptions, cancellation)
                .ConfigureAwait(false)
                ?? [];

            var duplicateSeedKey = entries
                .GroupBy(x => x.SeedKey, StringComparer.Ordinal)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateSeedKey is not null)
            {
                throw new InvalidOperationException($"Duplicate land parcel seed key '{duplicateSeedKey.Key}' in seed JSON.");
            }

            return entries;
        }
    }

    private sealed class LandParcelSeedEntry
    {
        public string SeedKey { get; set; } = string.Empty;

        public string TrustSeedKey { get; set; } = string.Empty;

        public string LegalParcelNumber { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public decimal AreaAcres { get; set; }

        public DateOnly AcquisitionDate { get; set; }

        public string AcquisitionMethod { get; set; } = string.Empty;

        public string? CovenantTerms { get; set; }
    }
}
