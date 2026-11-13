using System.Text.Json;
using CommonGround.Homeownership.Infrastructure.Seeding;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.Stewardship.Domain.StewardshipCases;
using CommonGround.Stewardship.Infrastructure.Persistence;
using CommonGround.Stewardship.Infrastructure.Seeding;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using JasperFx.Events;
using Marten;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>
/// Seeds what happened on the stewardship cases that seeded ground leases opened: occupancy
/// verifications, property inspections and lease violations, each step on its own date.
/// </summary>
public sealed class StewardshipCasesSeeder(ITrustClock trustClock, PropagationWaiter propagation) : IDevelopmentSeeder
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
                    $"Trust seed '{entry.TrustSeedKey}' not found for stewardship case seed '{entry.SeedKey}'. Ensure TrustsSeeder runs first.");

            using var trustSession = store.LightweightSession(trustLookup.TrustId);

            var lookupId = StewardshipCaseSeedLookup.BuildId(entry.SeedKey);
            var lookup = await trustSession.LoadAsync<StewardshipCaseSeedLookup>(lookupId, cancellation).ConfigureAwait(false);
            if (lookup is not null)
            {
                continue;
            }

            var timeZone = await trustClock.GetTimeZoneAsync(trustLookup.TrustId, cancellation).ConfigureAwait(false);
            var today = timeZone.ToDate(TimeProviderContext.Current.UtcNow);

            var caseView = await FindCaseAsync(trustSession, entry, cancellation).ConfigureAwait(false);
            var actions = Actions(entry, today);
            var outside = actions.Select(a => a.Date).Where(d => d < caseView.OpenedDate || d > today).ToList();
            if (outside.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Stewardship case seed '{entry.SeedKey}' has a step on {outside[0]}, outside its case's open period ({caseView.OpenedDate} to {today}).");
            }

            await SeedClock.RunAsync(timeZone, async clock =>
            {
                var stream = await trustSession.Events
                    .FetchForWriting<StewardshipCase>(caseView.Id, cancellation)
                    .ConfigureAwait(false);

                // Each step is recorded on its own date, in date order. The sort is stable, so steps
                // of one verification, inspection or violation on the same day keep their order.
                foreach (var (date, act) in actions.OrderBy(a => a.Date))
                {
                    clock.MoveTo(date);
                    act(stream);
                }

                trustSession.Store(new StewardshipCaseSeedLookup
                {
                    Id = lookupId,
                    SeedKey = entry.SeedKey,
                    StewardshipCaseId = caseView.Id,
                    SeededAt = DateTimeOffset.UtcNow,
                });

                await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
                await propagation.WaitForIdleAsync(cancellation).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }

    private static List<(DateOnly Date, Action<IEventStream<StewardshipCase>> Act)> Actions(StewardshipCaseSeedEntry entry, DateOnly today)
    {
        List<(DateOnly Date, Action<IEventStream<StewardshipCase>> Act)> actions = [];

        foreach (var verification in entry.Verifications)
        {
            // Each step needs the verification's ID, which only the initiating decision knows.
            var id = Guid.Empty;
            var periodYear = today.Year - verification.PeriodYearsAgo;
            var deadline = verification.SurveyDeadline?.Resolve(today);
            List<(DateOnly, Action<IEventStream<StewardshipCase>>)> steps =
            [
                (verification.Initiated.Resolve(today), stream => stream.Decide(sc =>
                {
                    var initiated = sc.InitiateOccupancyVerification(periodYear, deadline, SeederUserId);
                    id = initiated.VerificationId;
                    return initiated;
                })),
            ];

            if (verification.MarkedOverdue is { } overdue)
            {
                var deadlinePassed = deadline
                    ?? throw new InvalidOperationException($"Stewardship case seed '{entry.SeedKey}' has an overdue verification with no survey deadline.");
                steps.Add((overdue.Resolve(today), stream => stream.Decide(sc => sc.MarkOccupancyVerificationOverdue(id, deadlinePassed))));
            }

            if (verification.Submitted is { } submitted)
            {
                var on = submitted.Date.Resolve(today);
                steps.Add((on, stream => stream.Decide(sc =>
                    sc.SubmitOccupancyVerification(id, submitted.IsPrimaryResident, submitted.SubletDisclosed, submitted.MajorChanges, on))));
            }

            if (verification.Verified is { } verified)
            {
                var on = verified.Resolve(today);
                steps.Add((on, stream => stream.Decide(sc => sc.VerifyOccupancy(id, on, SeederUserId))));
            }

            if (verification.Failed is { } failed)
            {
                var on = failed.Date.Resolve(today);
                steps.Add((on, stream => stream.Decide(sc => sc.FailOccupancy(id, failed.Reason, on, SeederUserId))));
            }

            AddInOrder(actions, steps, entry, $"verification for {periodYear}");
        }

        foreach (var inspection in entry.Inspections)
        {
            var id = Guid.Empty;
            var scheduledFor = inspection.ScheduledFor.Resolve(today);
            List<(DateOnly, Action<IEventStream<StewardshipCase>>)> steps =
            [
                (inspection.Scheduled.Resolve(today), stream => stream.Decide(sc =>
                {
                    var scheduled = sc.SchedulePropertyInspection(scheduledFor, inspection.InspectorName, SeederUserId);
                    id = scheduled.InspectionId;
                    return scheduled;
                })),
            ];

            if (inspection.Completed is { } completed)
            {
                var on = completed.Date.Resolve(today);
                steps.Add((on, stream => stream.Decide(sc =>
                    sc.CompletePropertyInspection(id, completed.OccupancyConfirmed, completed.ConditionNotes, on, SeederUserId))));
            }

            if (inspection.Cancelled is { } cancelled)
            {
                steps.Add((cancelled.Date.Resolve(today), stream => stream.Decide(sc => sc.CancelPropertyInspection(id, cancelled.Reason, SeederUserId))));
            }

            AddInOrder(actions, steps, entry, $"inspection by {inspection.InspectorName} on {scheduledFor}");
        }

        foreach (var violation in entry.Violations)
        {
            var id = Guid.Empty;
            var type = LeaseViolationType.FromDisplayName(violation.Type);
            var identifiedOn = violation.Identified.Resolve(today);
            List<(DateOnly, Action<IEventStream<StewardshipCase>>)> steps =
            [
                (identifiedOn, stream => stream.Decide(sc =>
                {
                    var identified = sc.IdentifyViolation(type, violation.Description, identifiedOn, SeederUserId);
                    id = identified.ViolationId;
                    return identified;
                })),
            ];

            if (violation.Notice is { } notice)
            {
                var on = notice.Date.Resolve(today);
                var cureDeadline = notice.CureDeadline.Resolve(today);
                steps.Add((on, stream => stream.Decide(sc => sc.IssueViolationNotice(id, on, cureDeadline, SeederUserId))));
            }

            if (violation.Cured is { } cured)
            {
                var on = cured.Resolve(today);
                steps.Add((on, stream => stream.Decide(sc => sc.CureViolation(id, on, SeederUserId))));
            }

            if (violation.Escalated is { } escalated)
            {
                var on = escalated.Date.Resolve(today);
                steps.Add((on, stream => stream.Decide(sc => sc.EscalateViolation(id, escalated.Reason, on, SeederUserId))));
            }

            AddInOrder(actions, steps, entry, $"{violation.Type} violation identified on {identifiedOn}");
        }

        return actions;
    }

    // A record's steps are listed in the order they happen; their dates must agree.
    private static void AddInOrder(
        List<(DateOnly Date, Action<IEventStream<StewardshipCase>> Act)> actions,
        List<(DateOnly Date, Action<IEventStream<StewardshipCase>> Act)> steps,
        StewardshipCaseSeedEntry entry,
        string record)
    {
        for (var i = 1; i < steps.Count; i++)
        {
            if (steps[i].Date < steps[i - 1].Date)
            {
                throw new InvalidOperationException($"Stewardship case seed '{entry.SeedKey}' has the steps of its {record} out of date order.");
            }
        }

        actions.AddRange(steps);
    }

    private static async Task<IReadOnlyList<StewardshipCaseSeedEntry>> LoadEntriesAsync(CancellationToken cancellation)
    {
        var assembly = typeof(StewardshipCasesSeeder).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith("stewardship-cases.seed.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new InvalidOperationException("Could not find embedded stewardship case seed resource 'stewardship-cases.seed.json'.");
        }

        var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not load seed resource stream '{resourceName}'.");

        using (stream)
        {
            var entries = await JsonSerializer.DeserializeAsync<List<StewardshipCaseSeedEntry>>(stream, JsonOptions, cancellation)
                .ConfigureAwait(false)
                ?? [];

            var duplicate = entries
                .GroupBy(x => x.SeedKey, StringComparer.Ordinal)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicate is not null)
            {
                throw new InvalidOperationException($"Duplicate stewardship case seed key '{duplicate.Key}' in seed JSON.");
            }

            return entries;
        }
    }

    private async Task<StewardshipCaseView> FindCaseAsync(
        IQuerySession session,
        StewardshipCaseSeedEntry entry,
        CancellationToken cancellation)
    {
        var leaseLookup = await session
            .LoadAsync<GroundLeaseSeedLookup>(GroundLeaseSeedLookup.BuildId(entry.GroundLeaseSeedKey), cancellation)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException(
                $"Missing ground lease seed mapping for stewardship case seed key '{entry.SeedKey}' (groundLeaseSeedKey='{entry.GroundLeaseSeedKey}'). Ensure GroundLeasesSeeder runs first.");
        var leaseId = leaseLookup.GroundLeaseId;

        return await propagation.WaitForAsync(
            $"stewardship case for ground lease {leaseId}",
            ct => session.Query<StewardshipCaseView>()
                .Where(v => v.GroundLeaseId == leaseId)
                .FirstOrDefaultAsync(ct),
            cancellation).ConfigureAwait(false);
    }

    private sealed class StewardshipCaseSeedEntry
    {
        public string SeedKey { get; set; } = string.Empty;

        public string TrustSeedKey { get; set; } = string.Empty;

        public string GroundLeaseSeedKey { get; set; } = string.Empty;

        public List<VerificationSeedEntry> Verifications { get; set; } = [];

        public List<InspectionSeedEntry> Inspections { get; set; } = [];

        public List<ViolationSeedEntry> Violations { get; set; } = [];
    }

    private sealed class VerificationSeedEntry
    {
        // Relative, like the dates, so the case keeps a verification for the current year.
        public int PeriodYearsAgo { get; set; }

        public SeedDate Initiated { get; set; }

        public SeedDate? SurveyDeadline { get; set; }

        public SeedDate? MarkedOverdue { get; set; }

        public SubmissionSeedEntry? Submitted { get; set; }

        public SeedDate? Verified { get; set; }

        public ReasonSeedEntry? Failed { get; set; }
    }

    private sealed class InspectionSeedEntry
    {
        public SeedDate Scheduled { get; set; }

        public SeedDate ScheduledFor { get; set; }

        public string InspectorName { get; set; } = string.Empty;

        public CompletionSeedEntry? Completed { get; set; }

        public ReasonSeedEntry? Cancelled { get; set; }
    }

    private sealed class ViolationSeedEntry
    {
        public string Type { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public SeedDate Identified { get; set; }

        public NoticeSeedEntry? Notice { get; set; }

        public SeedDate? Cured { get; set; }

        public ReasonSeedEntry? Escalated { get; set; }
    }

    private class DatedSeedEntry
    {
        public SeedDate Date { get; set; }
    }

    private sealed class SubmissionSeedEntry : DatedSeedEntry
    {
        public bool IsPrimaryResident { get; set; }

        public bool SubletDisclosed { get; set; }

        public string? MajorChanges { get; set; }
    }

    private sealed class CompletionSeedEntry : DatedSeedEntry
    {
        public bool OccupancyConfirmed { get; set; }

        public string? ConditionNotes { get; set; }
    }

    private sealed class NoticeSeedEntry : DatedSeedEntry
    {
        public SeedDate CureDeadline { get; set; }
    }

    private sealed class ReasonSeedEntry : DatedSeedEntry
    {
        public string Reason { get; set; } = string.Empty;
    }
}
