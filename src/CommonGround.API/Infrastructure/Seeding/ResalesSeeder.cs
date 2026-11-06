using System.Text.Json;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.Resales;
using CommonGround.Homeownership.Infrastructure.Seeding;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using Marten;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>
/// Seeds resales on seeded ground leases: the homeowner's notice of intent to sell, on its own date, and
/// for some a later withdrawal, written to the lease and the resale as the app does.
/// </summary>
public sealed class ResalesSeeder(ITrustClock trustClock, PropagationWaiter propagation) : IDevelopmentSeeder
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
                    $"Trust seed '{entry.TrustSeedKey}' not found for resale seed '{entry.SeedKey}'. Ensure TrustsSeeder runs first.");

            using var trustSession = store.LightweightSession(trustLookup.TrustId);

            var lookupId = ResaleSeedLookup.BuildId(entry.SeedKey);
            var lookup = await trustSession.LoadAsync<ResaleSeedLookup>(lookupId, cancellation).ConfigureAwait(false);
            if (lookup is not null)
            {
                continue;
            }

            var leaseLookup = await trustSession
                .LoadAsync<GroundLeaseSeedLookup>(GroundLeaseSeedLookup.BuildId(entry.GroundLeaseSeedKey), cancellation)
                .ConfigureAwait(false)
                ?? throw new InvalidOperationException(
                    $"Missing ground lease seed mapping for resale seed key '{entry.SeedKey}' (groundLeaseSeedKey='{entry.GroundLeaseSeedKey}'). Ensure GroundLeasesSeeder runs first.");

            var timeZone = await trustClock.GetTimeZoneAsync(trustLookup.TrustId, cancellation).ConfigureAwait(false);
            var today = timeZone.ToDate(TimeProviderContext.Current.UtcNow);
            var noticeDate = entry.NoticeDate.Resolve(today);
            var withdrawnDate = entry.Withdrawn?.Date.Resolve(today);
            if (withdrawnDate < noticeDate)
            {
                throw new InvalidOperationException($"Resale seed '{entry.SeedKey}' is withdrawn before its notice.");
            }

            await SeedClock.RunAsync(timeZone, async clock =>
            {
                var resaleId = ResaleId.Generate();
                clock.MoveTo(noticeDate);
                var lease = await trustSession.Events
                    .FetchForWriting<GroundLease>(leaseLookup.GroundLeaseId, cancellation)
                    .ConfigureAwait(false);
                var recordedOn = timeZone.ToDate(lease.Aggregate!.CreatedAt);
                if (noticeDate < recordedOn)
                {
                    throw new InvalidOperationException(
                        $"Resale seed '{entry.SeedKey}' gives notice on {noticeDate}, before its lease was recorded on {recordedOn}.");
                }

                lease.Decide(l => l.StartResale(resaleId));
                trustSession.Events.StartStream<Resale>(
                    resaleId.Value,
                    Resale.RecordIntentToSell(
                        resaleId, lease.Aggregate!, noticeDate, NoticeChannel.FromDisplayName(entry.Channel), entry.Notes, noticeDate, SeederUserId));
                await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);

                if (entry.Withdrawn is { } withdrawn)
                {
                    clock.MoveTo(withdrawnDate!.Value);
                    var resale = await trustSession.Events
                        .FetchForWriting<Resale>(resaleId.Value, cancellation)
                        .ConfigureAwait(false);
                    lease = await trustSession.Events
                        .FetchForWriting<GroundLease>(leaseLookup.GroundLeaseId, cancellation)
                        .ConfigureAwait(false);
                    resale.Decide(r => r.Withdraw(withdrawnDate.Value, withdrawn.Reason, withdrawnDate.Value, SeederUserId));
                    lease.Decide(l => l.CloseResale(resaleId));
                }

                trustSession.Store(new ResaleSeedLookup
                {
                    Id = lookupId,
                    SeedKey = entry.SeedKey,
                    ResaleId = resaleId.Value,
                    SeededAt = DateTimeOffset.UtcNow,
                });

                await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
                await propagation.WaitForIdleAsync(cancellation).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }

    private static async Task<IReadOnlyList<ResaleSeedEntry>> LoadEntriesAsync(CancellationToken cancellation)
    {
        var assembly = typeof(ResalesSeeder).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith(".resales.seed.json", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("Could not find embedded resale seed resource 'resales.seed.json'.");

        var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not load seed resource stream '{resourceName}'.");

        using (stream)
        {
            var entries = await JsonSerializer.DeserializeAsync<List<ResaleSeedEntry>>(stream, JsonOptions, cancellation)
                .ConfigureAwait(false)
                ?? [];

            var duplicate = entries
                .GroupBy(x => x.SeedKey, StringComparer.Ordinal)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicate is not null)
            {
                throw new InvalidOperationException($"Duplicate resale seed key '{duplicate.Key}' in seed JSON.");
            }

            return entries;
        }
    }

    private sealed class ResaleSeedEntry
    {
        public string SeedKey { get; set; } = string.Empty;

        public string TrustSeedKey { get; set; } = string.Empty;

        public string GroundLeaseSeedKey { get; set; } = string.Empty;

        public SeedDate NoticeDate { get; set; }

        public string Channel { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public WithdrawnSeedEntry? Withdrawn { get; set; }
    }

    private sealed class WithdrawnSeedEntry
    {
        public SeedDate Date { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}
