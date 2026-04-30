using System.Text.Json;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.PropertyRegistry.Infrastructure.Persistence;
using CommonGround.SharedKernel.Domain;
using Marten;
using Marten.Schema;
using Microsoft.Extensions.Logging.Abstractions;

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
        var repository = new HomeRepository(session, NullLogger<HomeRepository>.Instance);

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

            var lookupId = HomeSeedLookup.BuildId(entry.SeedKey);
            var lookup = await session.LoadAsync<HomeSeedLookup>(lookupId, cancellation).ConfigureAwait(false);
            if (lookup is not null)
            {
                var streamState = await session.Events.FetchStreamStateAsync(lookup.UnitId, cancellation).ConfigureAwait(false);
                if (streamState is not null)
                {
                    continue;
                }
            }

            var unit = Home.Register(
                LandParcelId.Create(landParcelLookup.LandParcelId),
                HomeType.FromDisplayName(entry.Type),
                entry.Bedrooms,
                entry.Bathrooms,
                entry.SquareFootage,
                entry.YearBuilt,
                entry.InitialAssessedValue,
                SeederUserId);

            repository.Stage(unit);

            session.Store(new HomeSeedLookup
            {
                Id = lookupId,
                SeedKey = entry.SeedKey,
                UnitId = unit.Id.Value,
                SeededAt = DateTimeOffset.UtcNow,
            });

            await session.SaveChangesAsync(cancellation).ConfigureAwait(false);
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

    private sealed record HomeSeedEntry(
        string SeedKey,
        string LandParcelSeedKey,
        string Type,
        int Bedrooms,
        decimal Bathrooms,
        decimal SquareFootage,
        int YearBuilt,
        decimal InitialAssessedValue);
}
