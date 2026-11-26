using System.Text.Json;
using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Infrastructure.Seeding;
using CommonGround.Homeownership.Infrastructure.Seeding;
using CommonGround.PropertyRegistry.Infrastructure.Seeding;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using Marten;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>
/// Seeds development home selections from JSON: households chosen for homes by priority or by lottery. Choosing a
/// household selects it for the home, and propagation puts the home under contract and starts the household's purchase
/// workflow.
/// </summary>
/// <remarks>
/// Selections are seeded in the order listed, which is the order they were chosen, each one propagated before the next,
/// so every choice compares the households waiting on its date.
/// </remarks>
public sealed class HomeSelectionsSeeder(ITrustClock trustClock, PropagationWaiter propagation) : IDevelopmentSeeder
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
                    $"Trust seed '{entry.TrustSeedKey}' not found for home selection seed '{entry.SeedKey}'. Ensure TrustsSeeder runs first.");

            using var trustSession = store.LightweightSession(trustLookup.TrustId);

            var lookupId = HomeSelectionSeedLookup.BuildId(entry.SeedKey);
            var lookup = await trustSession.LoadAsync<HomeSelectionSeedLookup>(lookupId, cancellation).ConfigureAwait(false);
            if (lookup is not null)
            {
                var streamState = await trustSession.Events.FetchStreamStateAsync(lookup.HomeSelectionId, cancellation).ConfigureAwait(false);
                if (streamState is not null)
                {
                    continue;
                }
            }

            var homeLookup = await trustSession
                .LoadAsync<HomeSeedLookup>(HomeSeedLookup.BuildId(entry.HomeSeedKey), cancellation)
                .ConfigureAwait(false)
                ?? throw new InvalidOperationException(
                    $"Missing home seed mapping for home selection seed key '{entry.SeedKey}' (homeSeedKey='{entry.HomeSeedKey}').");
            var homeId = homeLookup.UnitId;

            var timeZone = await trustClock.GetTimeZoneAsync(trustLookup.TrustId, cancellation).ConfigureAwait(false);
            var today = timeZone.ToDate(TimeProviderContext.Current.UtcNow);

            await SeedClock.RunAsync(timeZone, async clock =>
            {
                var selectionId = HomeSelectionId.Generate();
                var selectedDate = (entry.Priority?.SelectedDate ?? entry.Lottery!.SelectedDate).Resolve(today);
                clock.MoveTo(selectedDate);

                var choice = await HomeSelectionChoice
                    .LoadAsync(trustSession, trustClock, homeId, selectedDate, cancellation)
                    .ConfigureAwait(false);
                HomeSelectionChoice.EnsureOffered(choice.Home, selectedDate);

                HomeownerApplicationId selectedApplicationId;
                if (entry.Priority is { } priority)
                {
                    selectedApplicationId = await LoadApplicationIdAsync(trustSession, priority.HouseholdSeedKey, entry.SeedKey, cancellation).ConfigureAwait(false);
                    var templateId = await LoadTemplateIdAsync(trustSession, priority.GroundLeaseTemplateSeedKey, entry.SeedKey, cancellation).ConfigureAwait(false);
                    var terms = new SelectionTerms(templateId, priority.PurchasePrice, selectedDate);

                    var chosen = HomeSelection.SelectByPriority(
                        selectionId,
                        homeId,
                        choice.Comparison,
                        selectedApplicationId,
                        [.. priority.Priorities.Select(SelectionPriority.FromDisplayName)],
                        priority.Reason,
                        terms,
                        choice.Today,
                        SeederUserId);
                    await HomeSelectionChoice
                        .RecordAsync(trustSession, choice, selectionId, chosen, selectedApplicationId, terms, SeederUserId, cancellation)
                        .ConfigureAwait(false);
                }
                else
                {
                    var lottery = entry.Lottery!;
                    List<HomeownerApplicationId> tiedApplicationIds = [];
                    foreach (var householdSeedKey in lottery.TiedHouseholdSeedKeys)
                    {
                        tiedApplicationIds.Add(await LoadApplicationIdAsync(trustSession, householdSeedKey, entry.SeedKey, cancellation).ConfigureAwait(false));
                    }

                    var templateId = await LoadTemplateIdAsync(trustSession, lottery.GroundLeaseTemplateSeedKey, entry.SeedKey, cancellation).ConfigureAwait(false);
                    var terms = new SelectionTerms(templateId, lottery.PurchasePrice, selectedDate);

                    // The handler draws with a fresh random seed; the seeder draws with the entry's seed so the
                    // winner is repeatable.
                    var drawn = HomeSelection.DrawLottery(
                        selectionId,
                        homeId,
                        choice.Comparison,
                        tiedApplicationIds,
                        lottery.LotterySeed,
                        terms,
                        choice.Today,
                        SeederUserId);
                    selectedApplicationId = HomeownerApplicationId.Create(drawn.ApplicationId);
                    await HomeSelectionChoice
                        .RecordAsync(trustSession, choice, selectionId, drawn, selectedApplicationId, terms, SeederUserId, cancellation)
                        .ConfigureAwait(false);
                }

                trustSession.Store(new HomeSelectionSeedLookup
                {
                    Id = lookupId,
                    SeedKey = entry.SeedKey,
                    HomeSelectionId = selectionId.Value,
                    SelectedApplicationId = selectedApplicationId.Value,
                    SeededAt = DateTimeOffset.UtcNow,
                });

                await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
                await propagation.WaitForIdleAsync(cancellation).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }

    private static async Task<HomeownerApplicationId> LoadApplicationIdAsync(
        IQuerySession session,
        string householdSeedKey,
        string selectionSeedKey,
        CancellationToken cancellation)
    {
        var lookup = await session
            .LoadAsync<HouseholdSeedLookup>(HouseholdSeedLookup.BuildId(householdSeedKey), cancellation)
            .ConfigureAwait(false);

        return lookup?.ApplicationId is { } applicationId
            ? HomeownerApplicationId.Create(applicationId)
            : throw new InvalidOperationException(
                $"Missing household application seed mapping for home selection seed key '{selectionSeedKey}' (householdSeedKey='{householdSeedKey}').");
    }

    private static async Task<string> LoadTemplateIdAsync(
        IQuerySession session,
        string templateSeedKey,
        string selectionSeedKey,
        CancellationToken cancellation)
    {
        var lookup = await session
            .LoadAsync<GroundLeaseTemplateSeedLookup>(GroundLeaseTemplateSeedLookup.BuildId(templateSeedKey), cancellation)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException(
                $"Missing ground lease template seed mapping for home selection seed key '{selectionSeedKey}' (groundLeaseTemplateSeedKey='{templateSeedKey}').");

        return lookup.TemplateId;
    }

    private static async Task<IReadOnlyList<HomeSelectionSeedEntry>> LoadEntriesAsync(CancellationToken cancellation)
    {
        var assembly = typeof(HomeSelectionsSeeder).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith("home-selections.seed.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new InvalidOperationException("Could not find embedded home selection seed resource 'home-selections.seed.json'.");
        }

        var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not load seed resource stream '{resourceName}'.");

        using (stream)
        {
            var entries = await JsonSerializer.DeserializeAsync<List<HomeSelectionSeedEntry>>(stream, JsonOptions, cancellation)
                .ConfigureAwait(false)
                ?? [];

            var duplicate = entries
                .GroupBy(x => x.SeedKey, StringComparer.Ordinal)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicate is not null)
            {
                throw new InvalidOperationException($"Duplicate home selection seed key '{duplicate.Key}' in seed JSON.");
            }

            var outcomes = entries.FirstOrDefault(e => (e.Priority is null) == (e.Lottery is null));
            if (outcomes is not null)
            {
                throw new InvalidOperationException($"Home selection seed '{outcomes.SeedKey}' needs exactly one of priority and lottery.");
            }

            return entries;
        }
    }

    private sealed class HomeSelectionSeedEntry
    {
        public string SeedKey { get; set; } = string.Empty;

        public string TrustSeedKey { get; set; } = string.Empty;

        public string HomeSeedKey { get; set; } = string.Empty;

        public PrioritySeedEntry? Priority { get; set; }

        public LotterySeedEntry? Lottery { get; set; }
    }

    private sealed class PrioritySeedEntry
    {
        public string HouseholdSeedKey { get; set; } = string.Empty;

        public List<string> Priorities { get; set; } = [];

        public string Reason { get; set; } = string.Empty;

        public SeedDate SelectedDate { get; set; }

        public string GroundLeaseTemplateSeedKey { get; set; } = string.Empty;

        public decimal PurchasePrice { get; set; }
    }

    private sealed class LotterySeedEntry
    {
        public List<string> TiedHouseholdSeedKeys { get; set; } = [];

        public int LotterySeed { get; set; }

        public SeedDate SelectedDate { get; set; }

        public string GroundLeaseTemplateSeedKey { get; set; } = string.Empty;

        public decimal PurchasePrice { get; set; }
    }
}
