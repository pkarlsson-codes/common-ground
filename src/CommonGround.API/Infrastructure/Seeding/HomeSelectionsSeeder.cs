using System.Text.Json;
using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.HomeBuyers.Domain.IncomeLimitSchedules;
using CommonGround.HomeBuyers.Infrastructure.Seeding;
using CommonGround.Homeownership.Infrastructure.Seeding;
using CommonGround.PropertyRegistry.Application.ReadModels;
using CommonGround.PropertyRegistry.Infrastructure.Seeding;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using Marten;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>
/// Seeds development home selections from JSON: open, cancelled, and completed by priority or by
/// lottery. Completing one selects the household for the home, and propagation puts the home
/// under contract and starts the household's purchase workflow.
/// </summary>
/// <remarks>
/// Selections are seeded in the order listed, which is the order they started, each one finished
/// before the next starts, so every selection's candidates are the households waiting on its start date.
/// </remarks>
public sealed class HomeSelectionsSeeder(ITrustClock trustClock, PropagationWaiter propagation) : IDevelopmentSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly UserId SeederUserId = UserId.Create("user-seeder-system");

    private static readonly string[] ActiveStatuses =
        [HomeSelectionStatus.Open.DisplayName, HomeSelectionStatus.Completed.DisplayName];

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
                var startedDate = entry.StartedDate.Resolve(today);
                clock.MoveTo(startedDate);

                var candidates = await LoadCandidatesAsync(trustSession, startedDate, cancellation).ConfigureAwait(false);
                var activeSelectionId = await trustSession.Query<HomeSelectionView>()
                    .Where(v => v.HomeId == homeId && ActiveStatuses.Contains(v.Status))
                    .Select(v => v.Id)
                    .FirstOrDefaultAsync(cancellation)
                    .ConfigureAwait(false);

                var home = await trustSession
                    .LoadAsync<HomeFactsView>(homeId, cancellation)
                    .ConfigureAwait(false)
                    ?? throw new InvalidOperationException($"HomeBuyers has no facts for home {homeId} (seed key '{entry.HomeSeedKey}').");
                var scheduleInEffect = await LoadScheduleInEffectAsync(trustSession, startedDate, cancellation).ConfigureAwait(false);

                var started = HomeSelection.Start(
                    selectionId,
                    homeId,
                    candidates,
                    home.IncomeLimitPercentOfAmi,
                    scheduleInEffect,
                    startedDate,
                    EntityIds.CreateOrNull<HomeSelectionId>(activeSelectionId),
                    SeederUserId);
                trustSession.Events.StartStream<HomeSelection>(selectionId.Value, started);
                await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);

                // PropertyRegistry shows the home in selection before anyone is chosen for it. Idle alone
                // has let the choice race the start here, so wait for the start to reach the home.
                await propagation.WaitForAsync(
                    $"home {homeId} to show selection {selectionId.Value} started",
                    async ct =>
                    {
                        var availability = await trustSession.LoadAsync<HomeAvailabilityView>(homeId, ct).ConfigureAwait(false);
                        return availability?.Selections.Any(s => s.Id == selectionId.Value && s.StartedAt is not null) == true ? availability : null;
                    },
                    cancellation).ConfigureAwait(false);
                await propagation.WaitForIdleAsync(cancellation).ConfigureAwait(false);

                string? selectedApplicationId = null;
                if (entry.Priority is { } priority)
                {
                    var selectedDate = priority.SelectedDate.Resolve(today);
                    clock.MoveTo(selectedDate);
                    var applicationId = await LoadApplicationIdAsync(trustSession, priority.HouseholdSeedKey, entry.SeedKey, cancellation).ConfigureAwait(false);
                    var templateId = await LoadTemplateIdAsync(trustSession, priority.GroundLeaseTemplateSeedKey, entry.SeedKey, cancellation).ConfigureAwait(false);

                    var selection = await HomeSelectionCompletion.FetchSelectionAsync(trustSession, selectionId, cancellation).ConfigureAwait(false);
                    var current = selection.Aggregate!;
                    selection.Decide(s => s.SelectByPriority(
                        applicationId,
                        [.. priority.Priorities.Select(SelectionPriority.FromDisplayName)],
                        priority.Reason,
                        SeederUserId));
                    var chosen = await HomeSelectionCompletion.FetchChosenAsync(trustSession, current, applicationId, cancellation).ConfigureAwait(false);
                    HomeSelectionCompletion.Complete(current, chosen, templateId, priority.PurchasePrice, selectedDate, SeederUserId);
                    selectedApplicationId = applicationId.Value;
                }
                else if (entry.Lottery is { } lottery)
                {
                    var selectedDate = lottery.SelectedDate.Resolve(today);
                    clock.MoveTo(selectedDate);
                    List<HomeownerApplicationId> tiedApplicationIds = [];
                    foreach (var householdSeedKey in lottery.TiedHouseholdSeedKeys)
                    {
                        tiedApplicationIds.Add(await LoadApplicationIdAsync(trustSession, householdSeedKey, entry.SeedKey, cancellation).ConfigureAwait(false));
                    }

                    var templateId = await LoadTemplateIdAsync(trustSession, lottery.GroundLeaseTemplateSeedKey, entry.SeedKey, cancellation).ConfigureAwait(false);

                    // The handler draws with a fresh random seed; the seeder draws with the entry's seed so
                    // the winner is repeatable, so it reuses only the handler's read of who is eligible.
                    var pool = await DrawSelectionLotteryHandler.LoadAsync(
                        new DrawSelectionLottery(selectionId, tiedApplicationIds, templateId, lottery.PurchasePrice, selectedDate, SeederUserId),
                        trustSession,
                        cancellation).ConfigureAwait(false);
                    var current = pool.Selection.Aggregate!;
                    var drawn = current.DrawLottery(tiedApplicationIds, pool.EligibleApplicationIds, lottery.LotterySeed, SeederUserId);
                    pool.Selection.AppendOne(drawn);

                    var winnerId = HomeownerApplicationId.Create(drawn.ApplicationId);
                    var chosen = await HomeSelectionCompletion.FetchChosenAsync(trustSession, current, winnerId, cancellation).ConfigureAwait(false);
                    HomeSelectionCompletion.Complete(current, chosen, templateId, lottery.PurchasePrice, selectedDate, SeederUserId);
                    selectedApplicationId = winnerId.Value;
                }
                else if (entry.Cancellation is { } cancellationEntry)
                {
                    clock.MoveTo(cancellationEntry.Date.Resolve(today));
                    var stream = await trustSession.Events
                        .FetchForWriting<HomeSelection>(selectionId.Value, cancellation)
                        .ConfigureAwait(false);
                    stream.Decide(selection => selection.Cancel(cancellationEntry.Reason, SeederUserId));
                }

                trustSession.Store(new HomeSelectionSeedLookup
                {
                    Id = lookupId,
                    SeedKey = entry.SeedKey,
                    HomeSelectionId = selectionId.Value,
                    SelectedApplicationId = selectedApplicationId,
                    SeededAt = DateTimeOffset.UtcNow,
                });

                await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
                await propagation.WaitForIdleAsync(cancellation).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }

    /// <summary>The income limit schedule in effect on <paramref name="day"/>, if any.</summary>
    private static async Task<IncomeLimitSchedule?> LoadScheduleInEffectAsync(
        IQuerySession session,
        DateOnly day,
        CancellationToken cancellation)
    {
        var schedules = await session.Query<IncomeLimitScheduleView>().ToListAsync(cancellation).ConfigureAwait(false);
        var inEffect = IncomeLimitScheduleView.InEffectOn(schedules, day);
        return inEffect is null
            ? null
            : await session.LoadAggregateAsync<IncomeLimitSchedule>(inEffect.Id, cancellation).ConfigureAwait(false);
    }

    /// <summary>The waiting list on <paramref name="startedDate"/>: approved applications approved by then, longest waiting first.</summary>
    private static async Task<IReadOnlyList<SelectionCandidate>> LoadCandidatesAsync(
        IQuerySession session,
        DateOnly startedDate,
        CancellationToken cancellation)
    {
        var approved = await session.Query<HomeownerApplicationView>()
            .Where(v => v.Phase == ApplicationPhase.Approved.DisplayName)
            .OrderBy(v => v.ApprovedDate)
            .ThenBy(v => v.SubmittedAt)
            .ToListAsync(cancellation)
            .ConfigureAwait(false);

        return [.. approved
            .Where(v => v.ApprovedDate <= startedDate)
            .Select(v => SelectionCandidate.Create(
                HomeownerApplicationId.Create(v.Id),
                HouseholdId.Create(v.HouseholdId),
                Money.Create(v.AnnualIncome),
                v.HouseholdSize,
                v.AccessibilityRequired,
                v.ApprovedDate!.Value,
                incomeLimit: null))];
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

            var outcomes = entries.FirstOrDefault(e => new object?[] { e.Priority, e.Lottery, e.Cancellation }.Count(o => o is not null) > 1);
            if (outcomes is not null)
            {
                throw new InvalidOperationException($"Home selection seed '{outcomes.SeedKey}' has more than one of priority, lottery and cancellation.");
            }

            return entries;
        }
    }

    private sealed class HomeSelectionSeedEntry
    {
        public string SeedKey { get; set; } = string.Empty;

        public string TrustSeedKey { get; set; } = string.Empty;

        public string HomeSeedKey { get; set; } = string.Empty;

        public SeedDate StartedDate { get; set; }

        public PrioritySeedEntry? Priority { get; set; }

        public LotterySeedEntry? Lottery { get; set; }

        public CancellationSeedEntry? Cancellation { get; set; }
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

    private sealed class CancellationSeedEntry
    {
        public SeedDate Date { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}
