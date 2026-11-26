using System.Text.Json;
using CommonGround.Reminders.Domain.Reminders;
using CommonGround.Reminders.Infrastructure.Seeding;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using Marten;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>
/// Seeds reminders from JSON, each set on its own date, and some marked done later,
/// written as the app does. Their dates are relative to today, so some show on Today, some are upcoming, one was done
/// lately and one was done too long ago for the Reminders page to list.
/// </summary>
public sealed class RemindersSeeder(ITrustClock trustClock, PropagationWaiter propagation) : IDevelopmentSeeder
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
                    $"Trust seed '{entry.TrustSeedKey}' not found for reminder seed '{entry.SeedKey}'. Ensure TrustsSeeder runs first.");

            using var trustSession = store.LightweightSession(trustLookup.TrustId);

            var lookupId = ReminderSeedLookup.BuildId(entry.SeedKey);
            if (await trustSession.LoadAsync<ReminderSeedLookup>(lookupId, cancellation).ConfigureAwait(false) is not null)
            {
                continue;
            }

            var timeZone = await trustClock.GetTimeZoneAsync(trustLookup.TrustId, cancellation).ConfigureAwait(false);
            var today = timeZone.ToDate(TimeProviderContext.Current.UtcNow);

            await SeedClock.RunAsync(timeZone, async clock =>
            {
                var reminderId = ReminderId.Generate();
                var setOn = entry.SetOn.Resolve(today);
                clock.MoveTo(setOn);

                trustSession.Events.StartStream<Reminder>(
                    reminderId.Value,
                    Reminder.Set(reminderId, entry.RemindOn.Resolve(today), entry.Note, entry.Section, setOn, SeederUserId));
                await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);

                if (entry.DoneOn is { } doneOn)
                {
                    clock.MoveTo(doneOn.Resolve(today));
                    var reminder = await trustSession.Events.FetchForWriting<Reminder>(reminderId.Value, cancellation).ConfigureAwait(false);
                    reminder.Decide(r => r.MarkDone(SeederUserId));
                    await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
                }

                trustSession.Store(new ReminderSeedLookup
                {
                    Id = lookupId,
                    SeedKey = entry.SeedKey,
                    ReminderId = reminderId.Value,
                    SeededAt = DateTimeOffset.UtcNow,
                });

                await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
                await propagation.WaitForIdleAsync(cancellation).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }

    private static async Task<IReadOnlyList<ReminderSeedEntry>> LoadEntriesAsync(CancellationToken cancellation)
    {
        var assembly = typeof(RemindersSeeder).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith(".reminders.seed.json", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("Could not find embedded reminder seed resource 'reminders.seed.json'.");

        var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not load seed resource stream '{resourceName}'.");

        using (stream)
        {
            var entries = await JsonSerializer.DeserializeAsync<List<ReminderSeedEntry>>(stream, JsonOptions, cancellation)
                .ConfigureAwait(false)
                ?? [];

            var duplicate = entries
                .GroupBy(x => x.SeedKey, StringComparer.Ordinal)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicate is not null)
            {
                throw new InvalidOperationException($"Duplicate reminder seed key '{duplicate.Key}' in seed JSON.");
            }

            return entries;
        }
    }

    private sealed class ReminderSeedEntry
    {
        public string SeedKey { get; set; } = string.Empty;

        public string TrustSeedKey { get; set; } = string.Empty;

        public SeedDate SetOn { get; set; }

        public SeedDate RemindOn { get; set; }

        public string? Section { get; set; }

        public string Note { get; set; } = string.Empty;

        public SeedDate? DoneOn { get; set; }
    }
}
