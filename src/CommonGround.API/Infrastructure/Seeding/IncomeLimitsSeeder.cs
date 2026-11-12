using System.Text.Json;
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.IncomeLimitSchedules;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using Marten;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>
/// Seeds development income limit schedules from JSON, last year's and this year's, so homes with an
/// income limit can start selections. Skips a schedule whose effective date the trust already has.
/// </summary>
public sealed class IncomeLimitsSeeder(ITrustClock trustClock) : IDevelopmentSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly UserId SeederUserId = UserId.Create("user-seeder-system");

    /// <inheritdoc/>
    public async Task SeedAsync(IDocumentStore store, CancellationToken cancellation)
    {
        ArgumentNullException.ThrowIfNull(store);

        var entries = await LoadEntriesAsync(cancellation).ConfigureAwait(false);
        using var defaultSession = store.LightweightSession();

        foreach (var entry in entries)
        {
            var trustLookup = await defaultSession
                .LoadAsync<TrustSeedLookup>(TrustSeedLookup.BuildId(entry.TrustSeedKey), cancellation)
                .ConfigureAwait(false)
                ?? throw new InvalidOperationException(
                    $"Trust seed '{entry.TrustSeedKey}' not found for an income limit schedule. Ensure TrustsSeeder runs first.");

            var timeZone = await trustClock.GetTimeZoneAsync(trustLookup.TrustId, cancellation).ConfigureAwait(false);
            var today = timeZone.ToDate(TimeProviderContext.Current.UtcNow);
            var effectiveFrom = entry.EffectiveFrom.Resolve(today);

            using var trustSession = store.LightweightSession(trustLookup.TrustId);
            var seeded = await trustSession.Query<IncomeLimitScheduleView>()
                .AnyAsync(v => v.EffectiveFrom == effectiveFrom, cancellation)
                .ConfigureAwait(false);
            if (seeded)
            {
                continue;
            }

            await SeedClock.RunAsync(timeZone, async clock =>
            {
                clock.MoveTo(entry.EnteredDate.Resolve(today));
                var id = IncomeLimitScheduleId.Generate();
                var entered = IncomeLimitSchedule.Enter(
                    id,
                    effectiveFrom,
                    [.. entry.Levels.Select(l => IncomeLimitLevel.Create(l.PercentOfAmi, [.. l.Limits.Select(Money.Create)]))],
                    scheduleOnSameDate: null,
                    SeederUserId);

                trustSession.Events.StartStream<IncomeLimitSchedule>(id.Value, entered);
                await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }

    private static async Task<IReadOnlyList<IncomeLimitScheduleSeedEntry>> LoadEntriesAsync(CancellationToken cancellation)
    {
        var assembly = typeof(IncomeLimitsSeeder).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith("income-limits.seed.json", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("Could not find embedded income limit seed resource 'income-limits.seed.json'.");

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not open embedded resource '{resourceName}'.");

        return await JsonSerializer.DeserializeAsync<List<IncomeLimitScheduleSeedEntry>>(stream, JsonOptions, cancellation).ConfigureAwait(false)
            ?? [];
    }

    private sealed class IncomeLimitScheduleSeedEntry
    {
        public string TrustSeedKey { get; init; } = string.Empty;

        public SeedDate EffectiveFrom { get; init; }

        public SeedDate EnteredDate { get; init; }

        public List<IncomeLimitLevelSeedEntry> Levels { get; init; } = [];
    }

    private sealed class IncomeLimitLevelSeedEntry
    {
        public int PercentOfAmi { get; init; }

        public List<decimal> Limits { get; init; } = [];
    }
}
