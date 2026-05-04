using System.Text.Json;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.PropertyRegistry.Infrastructure.Persistence;
using CommonGround.SharedKernel.Domain;
using Marten;
using Marten.Schema;
using Microsoft.Extensions.Logging.Abstractions;

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

        using var session = store.LightweightSession("org-dev-default");
        var repository = new LandParcelRepository(session, NullLogger<LandParcelRepository>.Instance);

        foreach (var entry in entries)
        {
            var lookupId = LandParcelSeedLookup.BuildId(entry.SeedKey);
            var lookup = await session.LoadAsync<LandParcelSeedLookup>(lookupId, cancellation).ConfigureAwait(false);
            if (lookup is not null)
            {
                var streamState = await session.Events.FetchStreamStateAsync(lookup.LandParcelId, cancellation).ConfigureAwait(false);
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

            repository.Stage(parcel);

            session.Store(new LandParcelSeedLookup
            {
                Id = lookupId,
                SeedKey = entry.SeedKey,
                LandParcelId = parcel.Id.Value,
                SeededAt = DateTimeOffset.UtcNow,
            });

            await session.SaveChangesAsync(cancellation).ConfigureAwait(false);
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

    private sealed record LandParcelSeedEntry(
        string SeedKey,
        string LegalParcelNumber,
        string Address,
        decimal AreaAcres,
        DateOnly AcquisitionDate,
        string AcquisitionMethod,
        string? CovenantTerms);
}
