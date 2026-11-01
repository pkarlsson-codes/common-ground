using System.Text.Json;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.TrustManagement.Domain.Trusts;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using Marten;
using Marten.Schema;

namespace CommonGround.PropertyRegistry.Infrastructure.Seeding;

/// <summary>
/// Seeds development homes from JSON and persists deterministic seed-key mappings.
/// </summary>
public sealed class HomesInitialData : IInitialData
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly UserId SeederUserId = UserId.Create("user-seeder-system");

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
                    $"Trust seed '{entry.TrustSeedKey}' not found for home seed '{entry.SeedKey}'. Ensure TrustsInitialData runs first.");

            using var trustSession = store.LightweightSession(trustLookup.TrustId);

            var landParcelLookup = await trustSession
                .LoadAsync<LandParcelSeedLookup>(LandParcelSeedLookup.BuildId(entry.LandParcelSeedKey), cancellation)
                .ConfigureAwait(false);

            if (landParcelLookup is null)
            {
                throw new InvalidOperationException(
                    $"Missing land parcel seed mapping for home seed key '{entry.SeedKey}' (landParcelSeedKey='{entry.LandParcelSeedKey}').");
            }

            var landParcelStreamState = await trustSession.Events
                .FetchStreamStateAsync(landParcelLookup.LandParcelId, cancellation)
                .ConfigureAwait(false);

            if (landParcelStreamState is null)
            {
                throw new InvalidOperationException(
                    $"Land parcel stream '{landParcelLookup.LandParcelId}' does not exist for home seed key '{entry.SeedKey}'.");
            }

            var lookupId = HomeSeedLookup.BuildId(entry.SeedKey);
            var lookup = await trustSession.LoadAsync<HomeSeedLookup>(lookupId, cancellation).ConfigureAwait(false);
            if (lookup is not null)
            {
                var streamState = await trustSession.Events.FetchStreamStateAsync(lookup.UnitId, cancellation).ConfigureAwait(false);
                if (streamState is not null)
                {
                    continue;
                }
            }

            var now = TimeProviderContext.Current.UtcNow;
            // Not ITrustClock: it depends on the document store this seeder runs while building.
            var trust = await defaultSession.Events
                .AggregateStreamAsync<Trust>(trustLookup.TrustId, token: cancellation)
                .ConfigureAwait(false)
                ?? throw new InvalidOperationException($"Trust stream '{trustLookup.TrustId}' does not exist for home seed '{entry.SeedKey}'.");
            var today = trust.TimeZone.ToDate(now);
            var initialAssessment = UnitAssessment.Create(
                entry.InitialAssessedValue,
                new DateOnly(entry.YearBuilt, 1, 1),
                appraiserName: null,
                conditionNotes: null,
                SeederUserId,
                now,
                today);

            var unit = Home.Register(
                LandParcelId.Create(landParcelLookup.LandParcelId),
                entry.UnitDesignation,
                HomeType.FromDisplayName(entry.Type),
                entry.Bedrooms,
                entry.Bathrooms,
                entry.IsAccessible,
                entry.SquareFootage,
                entry.YearBuilt,
                initialAssessment,
                SeederUserId);

            trustSession.Events.StartStream<Home>(unit.Id.Value, unit.UncommittedEvents.ToArray());
            unit.UncommittedEvents.MarkAsCommitted();

            trustSession.Store(new HomeSeedLookup
            {
                Id = lookupId,
                SeedKey = entry.SeedKey,
                UnitId = unit.Id.Value,
                SeededAt = DateTimeOffset.UtcNow,
            });

            await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
        }
    }

    private static async Task<IReadOnlyList<HomeSeedEntry>> LoadEntriesAsync(CancellationToken cancellation)
    {
        var assembly = typeof(HomesInitialData).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith("homes.seed.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new InvalidOperationException("Could not find embedded home seed resource 'homes.seed.json'.");
        }

        var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not load seed resource stream '{resourceName}'.");

        using (stream)
        {
            var entries = await JsonSerializer.DeserializeAsync<List<HomeSeedEntry>>(stream, JsonOptions, cancellation)
                .ConfigureAwait(false)
                ?? [];

            var duplicateSeedKey = entries
                .GroupBy(x => x.SeedKey, StringComparer.Ordinal)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateSeedKey is not null)
            {
                throw new InvalidOperationException($"Duplicate home seed key '{duplicateSeedKey.Key}' in seed JSON.");
            }

            return entries;
        }
    }

    private sealed class HomeSeedEntry
    {
        public string SeedKey { get; set; } = string.Empty;

        public string TrustSeedKey { get; set; } = string.Empty;

        public string LandParcelSeedKey { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string? UnitDesignation { get; set; }

        public int Bedrooms { get; set; }

        public decimal Bathrooms { get; set; }

        public bool IsAccessible { get; set; }

        public decimal SquareFootage { get; set; }

        public int YearBuilt { get; set; }

        public decimal InitialAssessedValue { get; set; }
    }
}
