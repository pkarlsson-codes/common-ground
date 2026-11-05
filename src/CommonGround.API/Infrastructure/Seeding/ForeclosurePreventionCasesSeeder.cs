using System.Text.Json;
using CommonGround.Homeownership.Infrastructure.Seeding;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases;
using CommonGround.Stewardship.Infrastructure.Persistence;
using CommonGround.Stewardship.Infrastructure.Seeding;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using JasperFx.Events;
using Marten;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>
/// Seeds foreclosure prevention cases on seeded ground leases: what opened each one, the servicer
/// delinquencies, counseling referrals, forbearance requests and cure periods recorded on it, and how
/// it was resolved, each step on its own date.
/// </summary>
public sealed class ForeclosurePreventionCasesSeeder(ITrustClock trustClock, PropagationWaiter propagation) : IDevelopmentSeeder
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
                    $"Trust seed '{entry.TrustSeedKey}' not found for foreclosure prevention case seed '{entry.SeedKey}'. Ensure TrustsSeeder runs first.");

            using var trustSession = store.LightweightSession(trustLookup.TrustId);

            var lookupId = ForeclosurePreventionCaseSeedLookup.BuildId(entry.SeedKey);
            var lookup = await trustSession.LoadAsync<ForeclosurePreventionCaseSeedLookup>(lookupId, cancellation).ConfigureAwait(false);
            if (lookup is not null)
            {
                continue;
            }

            var timeZone = await trustClock.GetTimeZoneAsync(trustLookup.TrustId, cancellation).ConfigureAwait(false);
            var today = timeZone.ToDate(TimeProviderContext.Current.UtcNow);

            // The lease's stewardship case is open exactly while the lease is in force.
            var stewardshipCase = await FindStewardshipCaseAsync(trustSession, entry, cancellation).ConfigureAwait(false);
            var leaseEnd = stewardshipCase.ClosedDate ?? today;
            var trigger = Enum.Parse<ForeclosurePreventionTrigger>(entry.Trigger);
            var opened = entry.Opened.Resolve(today);
            var actions = Actions(entry, trigger, opened, today);
            var outside = actions.Select(a => a.Date).Prepend(opened).Where(d => d < stewardshipCase.OpenedDate || d > leaseEnd).ToList();
            if (outside.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Foreclosure prevention case seed '{entry.SeedKey}' has a step on {outside[0]}, outside its lease's term in CommonGround ({stewardshipCase.OpenedDate} to {leaseEnd}).");
            }

            await SeedClock.RunAsync(timeZone, async clock =>
            {
                var caseId = ForeclosurePreventionCaseId.Generate();
                clock.MoveTo(opened);
                trustSession.Events.StartStream<ForeclosurePreventionCase>(
                    caseId.Value,
                    ForeclosurePreventionCase.Open(caseId, stewardshipCase.GroundLeaseId, trigger, opened, SeederUserId));
                await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);

                var stream = await trustSession.Events
                    .FetchForWriting<ForeclosurePreventionCase>(caseId.Value, cancellation)
                    .ConfigureAwait(false);

                // Each step is recorded on its own date, in date order. The sort is stable, so steps
                // on the same day keep the order they are listed in.
                foreach (var (date, act) in actions.OrderBy(a => a.Date))
                {
                    clock.MoveTo(date);
                    act(stream);
                }

                trustSession.Store(new ForeclosurePreventionCaseSeedLookup
                {
                    Id = lookupId,
                    SeedKey = entry.SeedKey,
                    ForeclosurePreventionCaseId = caseId.Value,
                    SeededAt = DateTimeOffset.UtcNow,
                });

                await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
                await propagation.WaitForIdleAsync(cancellation).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }

    private static List<(DateOnly Date, Action<IEventStream<ForeclosurePreventionCase>> Act)> Actions(
        ForeclosurePreventionCaseSeedEntry entry,
        ForeclosurePreventionTrigger trigger,
        DateOnly opened,
        DateOnly today)
    {
        List<(DateOnly Date, Action<IEventStream<ForeclosurePreventionCase>> Act)> actions = [];

        // A case opened for a servicer delinquency records it on the day it opens, as the app does.
        if (trigger == ForeclosurePreventionTrigger.ServicerDelinquency
            && (entry.Delinquencies.Count == 0 || entry.Delinquencies[0].Date.Resolve(today) != opened))
        {
            throw new InvalidOperationException(
                $"Foreclosure prevention case seed '{entry.SeedKey}' was opened for a servicer delinquency, so its first delinquency must be on the day it opened.");
        }

        List<(DateOnly, Action<IEventStream<ForeclosurePreventionCase>>)> delinquencies = [];
        foreach (var delinquency in entry.Delinquencies)
        {
            var on = delinquency.Date.Resolve(today);
            delinquencies.Add((on, stream => stream.Decide(fpc =>
                fpc.RecordServicerDelinquency(delinquency.ServicerName, delinquency.DelinquentMonths, on, SeederUserId))));
        }

        AddInOrder(actions, delinquencies, entry, "servicer delinquencies");

        foreach (var referral in entry.CounselingReferrals)
        {
            // The outcome needs the referral's ID, which only the referring decision knows.
            var id = Guid.Empty;
            var referredOn = referral.Date.Resolve(today);
            List<(DateOnly, Action<IEventStream<ForeclosurePreventionCase>>)> steps =
            [
                (referredOn, stream => stream.Decide(fpc =>
                {
                    var made = fpc.MakeCounselingReferral(referral.AgencyName, referredOn, SeederUserId);
                    id = made.ReferralId;
                    return made;
                })),
            ];

            if (referral.Outcome is { } outcome)
            {
                var on = outcome.Date.Resolve(today);
                steps.Add((on, stream => stream.Decide(fpc => fpc.RecordCounselingOutcome(id, outcome.Outcome, on, SeederUserId))));
            }

            AddInOrder(actions, steps, entry, $"referral to {referral.AgencyName} on {referredOn}");
        }

        foreach (var request in entry.ForbearanceRequests)
        {
            var id = Guid.Empty;
            var requestedOn = request.Date.Resolve(today);
            List<(DateOnly, Action<IEventStream<ForeclosurePreventionCase>>)> steps =
            [
                (requestedOn, stream => stream.Decide(fpc =>
                {
                    var requested = fpc.RequestForbearance(request.LenderName, requestedOn, SeederUserId);
                    id = requested.RequestId;
                    return requested;
                })),
            ];

            if (request.Outcome is { } outcome)
            {
                var on = outcome.Date.Resolve(today);
                var decision = ForbearanceOutcome.FromDisplayName(outcome.Outcome);
                steps.Add((on, stream => stream.Decide(fpc => fpc.RecordForbearanceOutcome(id, decision, on, outcome.Notes, SeederUserId))));
            }

            AddInOrder(actions, steps, entry, $"forbearance request to {request.LenderName} on {requestedOn}");
        }

        foreach (var curePeriod in entry.CurePeriods)
        {
            var on = curePeriod.Date.Resolve(today);
            actions.Add((on, stream => stream.Decide(fpc => fpc.ActivateCurePeriod(curePeriod.LraId, on, curePeriod.CurePeriodDays, SeederUserId))));
        }

        if (entry.Resolved is { } resolved)
        {
            // Nothing happens on a case once it is resolved.
            var on = resolved.Date.Resolve(today);
            if (actions.Any(a => a.Date > on))
            {
                throw new InvalidOperationException($"Foreclosure prevention case seed '{entry.SeedKey}' has a step after it was resolved.");
            }

            var outcome = Enum.Parse<ForeclosurePreventionOutcome>(resolved.Outcome);
            actions.Add((on, stream => stream.Decide(fpc => fpc.Resolve(outcome, on, SeederUserId))));
        }

        return actions;
    }

    // A record's steps are listed in the order they happen; their dates must agree.
    private static void AddInOrder(
        List<(DateOnly Date, Action<IEventStream<ForeclosurePreventionCase>> Act)> actions,
        List<(DateOnly Date, Action<IEventStream<ForeclosurePreventionCase>> Act)> steps,
        ForeclosurePreventionCaseSeedEntry entry,
        string record)
    {
        for (var i = 1; i < steps.Count; i++)
        {
            if (steps[i].Date < steps[i - 1].Date)
            {
                throw new InvalidOperationException($"Foreclosure prevention case seed '{entry.SeedKey}' has the steps of its {record} out of date order.");
            }
        }

        actions.AddRange(steps);
    }

    private static async Task<IReadOnlyList<ForeclosurePreventionCaseSeedEntry>> LoadEntriesAsync(CancellationToken cancellation)
    {
        var assembly = typeof(ForeclosurePreventionCasesSeeder).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith("foreclosure-prevention-cases.seed.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new InvalidOperationException("Could not find embedded foreclosure prevention case seed resource 'foreclosure-prevention-cases.seed.json'.");
        }

        var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not load seed resource stream '{resourceName}'.");

        using (stream)
        {
            var entries = await JsonSerializer.DeserializeAsync<List<ForeclosurePreventionCaseSeedEntry>>(stream, JsonOptions, cancellation)
                .ConfigureAwait(false)
                ?? [];

            var duplicate = entries
                .GroupBy(x => x.SeedKey, StringComparer.Ordinal)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicate is not null)
            {
                throw new InvalidOperationException($"Duplicate foreclosure prevention case seed key '{duplicate.Key}' in seed JSON.");
            }

            return entries;
        }
    }

    private async Task<StewardshipCaseView> FindStewardshipCaseAsync(
        IQuerySession session,
        ForeclosurePreventionCaseSeedEntry entry,
        CancellationToken cancellation)
    {
        var leaseLookup = await session
            .LoadAsync<GroundLeaseSeedLookup>(GroundLeaseSeedLookup.BuildId(entry.GroundLeaseSeedKey), cancellation)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException(
                $"Missing ground lease seed mapping for foreclosure prevention case seed key '{entry.SeedKey}' (groundLeaseSeedKey='{entry.GroundLeaseSeedKey}'). Ensure GroundLeasesSeeder runs first.");
        var leaseId = leaseLookup.GroundLeaseId;

        return await propagation.WaitForAsync(
            $"stewardship case for ground lease {leaseId}",
            ct => session.Query<StewardshipCaseView>()
                .Where(v => v.GroundLeaseId == leaseId)
                .FirstOrDefaultAsync(ct),
            cancellation).ConfigureAwait(false);
    }

    private sealed class ForeclosurePreventionCaseSeedEntry
    {
        public string SeedKey { get; set; } = string.Empty;

        public string TrustSeedKey { get; set; } = string.Empty;

        public string GroundLeaseSeedKey { get; set; } = string.Empty;

        public string Trigger { get; set; } = string.Empty;

        public SeedDate Opened { get; set; }

        public List<DelinquencySeedEntry> Delinquencies { get; set; } = [];

        public List<ReferralSeedEntry> CounselingReferrals { get; set; } = [];

        public List<ForbearanceSeedEntry> ForbearanceRequests { get; set; } = [];

        public List<CurePeriodSeedEntry> CurePeriods { get; set; } = [];

        public OutcomeSeedEntry? Resolved { get; set; }
    }

    private class DatedSeedEntry
    {
        public SeedDate Date { get; set; }
    }

    private sealed class DelinquencySeedEntry : DatedSeedEntry
    {
        public string ServicerName { get; set; } = string.Empty;

        public int DelinquentMonths { get; set; }
    }

    private sealed class ReferralSeedEntry : DatedSeedEntry
    {
        public string AgencyName { get; set; } = string.Empty;

        public OutcomeSeedEntry? Outcome { get; set; }
    }

    private sealed class ForbearanceSeedEntry : DatedSeedEntry
    {
        public string LenderName { get; set; } = string.Empty;

        public OutcomeSeedEntry? Outcome { get; set; }
    }

    private sealed class CurePeriodSeedEntry : DatedSeedEntry
    {
        public string LraId { get; set; } = string.Empty;

        public int CurePeriodDays { get; set; }
    }

    private sealed class OutcomeSeedEntry : DatedSeedEntry
    {
        public string Outcome { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}
