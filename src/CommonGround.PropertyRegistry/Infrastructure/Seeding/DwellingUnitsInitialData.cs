using System.Text.Json;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.PropertyRegistry.Domain.LeasableUnits;
using CommonGround.PropertyRegistry.Infrastructure.Persistence;
using CommonGround.SharedKernel.Domain;
using Marten;
using Marten.Schema;
using Microsoft.Extensions.Logging.Abstractions;

namespace CommonGround.PropertyRegistry.Infrastructure.Seeding;

/// <summary>
/// Seeds development dwelling units from JSON and persists deterministic seed-key mappings.
/// </summary>
public sealed class DwellingUnitsInitialData : IInitialData
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

        using var session = store.LightweightSession();
        var repository = new LeasableUnitRepository(session, NullLogger<LeasableUnitRepository>.Instance);

        foreach (var entry in entries)
        {
            var landParcelLookup = await session
                .LoadAsync<LandParcelSeedLookup>(LandParcelSeedLookup.BuildId(entry.LandParcelSeedKey), cancellation)
                .ConfigureAwait(false);

            if (landParcelLookup is null)
            {
                throw new InvalidOperationException(
                    $"Missing land parcel seed mapping for unit seed key '{entry.SeedKey}' (landParcelSeedKey='{entry.LandParcelSeedKey}').");
            }

            var landParcelStreamState = await session.Events
                .FetchStreamStateAsync(landParcelLookup.LandParcelId, cancellation)
                .ConfigureAwait(false);

            if (landParcelStreamState is null)
            {
                throw new InvalidOperationException(
                    $"Land parcel stream '{landParcelLookup.LandParcelId}' does not exist for unit seed key '{entry.SeedKey}'.");
            }

            var lookupId = DwellingUnitSeedLookup.BuildId(entry.SeedKey);
            var lookup = await session.LoadAsync<DwellingUnitSeedLookup>(lookupId, cancellation).ConfigureAwait(false);
            if (lookup is not null)
            {
                var streamState = await session.Events.FetchStreamStateAsync(lookup.UnitId, cancellation).ConfigureAwait(false);
                if (streamState is not null)
                {
                    continue;
                }
            }

            var unit = DwellingUnit.Register(
                LandParcelId.Create(landParcelLookup.LandParcelId),
                DwellingUnitType.FromDisplayName(entry.Type),
                entry.Bedrooms,
                entry.Bathrooms,
                entry.SquareFootage,
                entry.YearBuilt,
                entry.InitialAssessedValue,
                SeederUserId);

            repository.Stage(unit);

            session.Store(new DwellingUnitSeedLookup
            {
                Id = lookupId,
                SeedKey = entry.SeedKey,
                UnitId = unit.Id.Value,
                SeededAt = DateTimeOffset.UtcNow,
            });

            await session.SaveChangesAsync(cancellation).ConfigureAwait(false);
        }
    }

    private static async Task<IReadOnlyList<DwellingUnitSeedEntry>> LoadEntriesAsync(CancellationToken cancellation)
    {
        var assembly = typeof(DwellingUnitsInitialData).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith("buildings.seed.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new InvalidOperationException("Could not find embedded building seed resource 'buildings.seed.json'.");
        }

        var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not load seed resource stream '{resourceName}'.");

        using (stream)
        {
            var entries = await JsonSerializer.DeserializeAsync<List<DwellingUnitSeedEntry>>(stream, JsonOptions, cancellation)
                .ConfigureAwait(false)
                ?? [];

            var duplicateSeedKey = entries
                .GroupBy(x => x.SeedKey, StringComparer.Ordinal)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateSeedKey is not null)
            {
                throw new InvalidOperationException($"Duplicate dwelling unit seed key '{duplicateSeedKey.Key}' in seed JSON.");
            }

            return entries;
        }
    }

    private sealed record DwellingUnitSeedEntry(
        string SeedKey,
        string LandParcelSeedKey,
        string Type,
        int Bedrooms,
        decimal Bathrooms,
        decimal SquareFootage,
        int YearBuilt,
        decimal InitialAssessedValue);
}
