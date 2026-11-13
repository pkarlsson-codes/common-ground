using System.Text.Json;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using Marten;

namespace CommonGround.PropertyRegistry.Infrastructure.Seeding;

/// <summary>
/// Seeds development land parcels from JSON and persists deterministic seed-key mappings.
/// </summary>
public sealed class LandParcelsSeeder(ITrustClock trustClock, PropagationWaiter propagation) : IDevelopmentSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly UserId SeederUserId = UserId.Create("user-seeder-system");

    /// <inheritdoc/>
    public async Task SeedAsync(SeedRun run, IDocumentStore store, CancellationToken cancellation)
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
                .LoadAsync<TrustSeedLookup>(TrustSeedLookup.BuildId(run, entry.TrustSeedKey), cancellation)
                .ConfigureAwait(false)
                ?? throw new InvalidOperationException(
                    $"Trust seed '{entry.TrustSeedKey}' not found for land parcel seed '{entry.SeedKey}'. Ensure TrustsSeeder runs first.");

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

            var timeZone = await trustClock.GetTimeZoneAsync(trustLookup.TrustId, cancellation).ConfigureAwait(false);
            var today = timeZone.ToDate(TimeProviderContext.Current.UtcNow);

            // The parcel is registered on the day it was acquired, and decommissioned on its own date.
            await SeedClock.RunAsync(timeZone, async clock =>
            {
                var parcelId = LandParcelId.Generate();
                var acquisitionDate = entry.AcquisitionDate.Resolve(today);
                clock.MoveTo(acquisitionDate);
                var registered = LandParcel.Register(
                    parcelId,
                    entry.LegalParcelNumber,
                    entry.Address,
                    entry.AreaAcres,
                    acquisitionDate,
                    AcquisitionMethod.FromDisplayName(entry.AcquisitionMethod),
                    entry.CovenantTerms,
                    SeederUserId);

                List<object> events = [registered];
                if (entry.DecommissionedDate is { } decommissioned)
                {
                    clock.MoveTo(decommissioned.Resolve(today));
                    events.Add(LandParcel.Create(registered).Decommission(SeederUserId)!);
                }

                trustSession.Events.StartStream<LandParcel>(parcelId.Value, events);

                trustSession.Store(new LandParcelSeedLookup
                {
                    Id = lookupId,
                    SeedKey = entry.SeedKey,
                    LandParcelId = parcelId.Value,
                    SeededAt = DateTimeOffset.UtcNow,
                });

                await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
                await propagation.WaitForIdleAsync(cancellation).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }

    private static async Task<IReadOnlyList<LandParcelSeedEntry>> LoadEntriesAsync(CancellationToken cancellation)
    {
        var assembly = typeof(LandParcelsSeeder).Assembly;
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

        public SeedDate AcquisitionDate { get; set; }

        public string AcquisitionMethod { get; set; } = string.Empty;

        public string? CovenantTerms { get; set; }

        public SeedDate? DecommissionedDate { get; set; }
    }
}
