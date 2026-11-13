using System.Text.Json;
using CommonGround.PropertyRegistry.Domain.DevelopmentProjects;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using Marten;

namespace CommonGround.PropertyRegistry.Infrastructure.Seeding;

/// <summary>
/// Seeds development projects from JSON: new construction on a land parcel, or rehabilitation of
/// homes on one.
/// </summary>
public sealed class DevelopmentProjectsSeeder(ITrustClock trustClock, PropagationWaiter propagation) : IDevelopmentSeeder
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
                    $"Trust seed '{entry.TrustSeedKey}' not found for development project seed '{entry.SeedKey}'. Ensure TrustsSeeder runs first.");

            using var trustSession = store.LightweightSession(trustLookup.TrustId);

            var lookupId = DevelopmentProjectSeedLookup.BuildId(entry.SeedKey);
            var lookup = await trustSession.LoadAsync<DevelopmentProjectSeedLookup>(lookupId, cancellation).ConfigureAwait(false);
            if (lookup is not null)
            {
                var streamState = await trustSession.Events.FetchStreamStateAsync(lookup.DevelopmentProjectId, cancellation).ConfigureAwait(false);
                if (streamState is not null)
                {
                    continue;
                }
            }

            var landParcelLookup = await trustSession
                .LoadAsync<LandParcelSeedLookup>(LandParcelSeedLookup.BuildId(entry.LandParcelSeedKey), cancellation)
                .ConfigureAwait(false)
                ?? throw new InvalidOperationException(
                    $"Missing land parcel seed mapping for development project seed key '{entry.SeedKey}' (landParcelSeedKey='{entry.LandParcelSeedKey}').");

            List<HomeId> homeIds = [];
            foreach (var homeSeedKey in entry.HomeSeedKeys)
            {
                var homeLookup = await trustSession
                    .LoadAsync<HomeSeedLookup>(HomeSeedLookup.BuildId(homeSeedKey), cancellation)
                    .ConfigureAwait(false)
                    ?? throw new InvalidOperationException(
                        $"Missing home seed mapping for development project seed key '{entry.SeedKey}' (homeSeedKey='{homeSeedKey}').");
                homeIds.Add(HomeId.Create(homeLookup.UnitId));
            }

            var timeZone = await trustClock.GetTimeZoneAsync(trustLookup.TrustId, cancellation).ConfigureAwait(false);
            var today = timeZone.ToDate(TimeProviderContext.Current.UtcNow);

            await SeedClock.RunAsync(timeZone, async clock =>
            {
                var projectId = DevelopmentProjectId.Generate();
                var landParcelId = LandParcelId.Create(landParcelLookup.LandParcelId);
                clock.MoveTo(entry.InitiatedDate.Resolve(today));
                var initiated = ProjectType.FromDisplayName(entry.Type) == ProjectType.Rehabilitation
                    ? DevelopmentProject.InitiateRehabilitation(projectId, landParcelId, entry.Name, entry.Description, homeIds, SeederUserId)
                    : DevelopmentProject.InitiateNewConstruction(projectId, landParcelId, entry.Name, entry.Description, SeederUserId);

                trustSession.Events.StartStream<DevelopmentProject>(projectId.Value, initiated);

                trustSession.Store(new DevelopmentProjectSeedLookup
                {
                    Id = lookupId,
                    SeedKey = entry.SeedKey,
                    DevelopmentProjectId = projectId.Value,
                    SeededAt = DateTimeOffset.UtcNow,
                });

                await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
                await propagation.WaitForIdleAsync(cancellation).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }

    private static async Task<IReadOnlyList<DevelopmentProjectSeedEntry>> LoadEntriesAsync(CancellationToken cancellation)
    {
        var assembly = typeof(DevelopmentProjectsSeeder).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith("development-projects.seed.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new InvalidOperationException("Could not find embedded development project seed resource 'development-projects.seed.json'.");
        }

        var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not load seed resource stream '{resourceName}'.");

        using (stream)
        {
            var entries = await JsonSerializer.DeserializeAsync<List<DevelopmentProjectSeedEntry>>(stream, JsonOptions, cancellation)
                .ConfigureAwait(false)
                ?? [];

            var duplicateSeedKey = entries
                .GroupBy(x => x.SeedKey, StringComparer.Ordinal)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateSeedKey is not null)
            {
                throw new InvalidOperationException($"Duplicate development project seed key '{duplicateSeedKey.Key}' in seed JSON.");
            }

            return entries;
        }
    }

    private sealed class DevelopmentProjectSeedEntry
    {
        public string SeedKey { get; set; } = string.Empty;

        public string TrustSeedKey { get; set; } = string.Empty;

        public string LandParcelSeedKey { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public List<string> HomeSeedKeys { get; set; } = [];

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public SeedDate InitiatedDate { get; set; }
    }
}
