using System.Text.Json;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Infrastructure.Seeding;
using CommonGround.PropertyRegistry.Infrastructure.Seeding;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using JasperFx.Events;
using Marten;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>
/// Seeds ground leases from JSON: what happened on the leases that closed purchases created
/// (stewardship check-ins, a resale's purchase or a termination, which propagates to close the
/// lease's stewardship case), and leases created directly, without a template or a purchase, which
/// propagate like those from a purchase: the lease opens a stewardship case and marks its home leased.
/// </summary>
public sealed class GroundLeasesSeeder(ITrustClock trustClock, PropagationWaiter propagation) : IDevelopmentSeeder
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
                    $"Trust seed '{entry.TrustSeedKey}' not found for ground lease seed '{entry.SeedKey}'. Ensure TrustsSeeder runs first.");

            using var trustSession = store.LightweightSession(trustLookup.TrustId);

            var lookupId = GroundLeaseSeedLookup.BuildId(entry.SeedKey);
            var lookup = await trustSession.LoadAsync<GroundLeaseSeedLookup>(lookupId, cancellation).ConfigureAwait(false);
            if (lookup is not null)
            {
                continue;
            }

            var timeZone = await trustClock.GetTimeZoneAsync(trustLookup.TrustId, cancellation).ConfigureAwait(false);
            var today = timeZone.ToDate(TimeProviderContext.Current.UtcNow);

            await SeedClock.RunAsync(timeZone, async clock =>
            {
                var leaseId = entry.Direct is { } direct
                    ? await EstablishAsync(trustSession, propagation, entry, direct, clock, today, cancellation).ConfigureAwait(false)
                    : await FindLeaseFromPurchaseAsync(trustSession, entry, cancellation).ConfigureAwait(false);

                var stream = await trustSession.Events
                    .FetchForWriting<GroundLease>(leaseId, cancellation)
                    .ConfigureAwait(false);

                // Each action is recorded on its own date, in date order, so the lease reads as having
                // been visited, put up for resale or ended then.
                foreach (var (date, act) in Actions(entry, today).OrderBy(a => a.Date))
                {
                    clock.MoveTo(date);
                    act(trustSession, stream);
                }

                trustSession.Store(new GroundLeaseSeedLookup
                {
                    Id = lookupId,
                    SeedKey = entry.SeedKey,
                    GroundLeaseId = leaseId,
                    SeededAt = DateTimeOffset.UtcNow,
                });

                await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
                await propagation.WaitForIdleAsync(cancellation).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }

    private static async Task<string> EstablishAsync(
        IDocumentSession session,
        PropagationWaiter propagation,
        GroundLeaseSeedEntry entry,
        DirectLeaseSeedEntry direct,
        SeedClock clock,
        DateOnly today,
        CancellationToken cancellation)
    {
        var parcelLookup = await session
            .LoadAsync<LandParcelSeedLookup>(LandParcelSeedLookup.BuildId(direct.LandParcelSeedKey), cancellation)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException(
                $"Missing land parcel seed mapping for ground lease seed key '{entry.SeedKey}' (landParcelSeedKey='{direct.LandParcelSeedKey}').");
        var homeLookup = await session
            .LoadAsync<HomeSeedLookup>(HomeSeedLookup.BuildId(direct.HomeSeedKey), cancellation)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException(
                $"Missing home seed mapping for ground lease seed key '{entry.SeedKey}' (homeSeedKey='{direct.HomeSeedKey}').");

        // Recorded later than it started, like a lease signed before the trust used CommonGround; its
        // stewardship case opens on the day it was recorded.
        var leaseStartDate = direct.LeaseStartDate.Resolve(today);
        clock.MoveTo(direct.RecordedOn.Resolve(today));

        var resale = direct.ResalePolicy;
        var leaseId = GroundLeaseId.Generate();
        var created = GroundLease.Establish(
            leaseId,
            parcelLookup.LandParcelId,
            homeLookup.UnitId,
            Homeowner.From(direct.Homeowner.FullName, direct.Homeowner.Email, direct.Homeowner.Phone),
            leaseStartDate,
            direct.TermYears,
            direct.OptionWindowDays,
            direct.BasePrice,
            direct.OriginalAppraisedValue,
            direct.CovenantTerms,
            ResalePolicyTerms.FromString(
                resale.FormulaKind,
                resale.AppreciationShare,
                resale.IndexType,
                resale.BaselineIndexValue,
                resale.AnnualPercentage,
                resale.ImprovementsCap),
            HomeCommitments.None,
            SeederUserId);
        session.Events.StartStream<GroundLease>(leaseId.Value, created);

        // Saved now so the lease can be fetched for writing like one a purchase created, and propagated
        // before the clock moves on, so its stewardship case opens on the day it was recorded.
        await session.SaveChangesAsync(cancellation).ConfigureAwait(false);
        await propagation.WaitForIdleAsync(cancellation).ConfigureAwait(false);

        return leaseId.Value;
    }

    private static List<(DateOnly Date, Action<IDocumentSession, IEventStream<GroundLease>> Act)> Actions(GroundLeaseSeedEntry entry, DateOnly today)
    {
        List<(DateOnly Date, Action<IDocumentSession, IEventStream<GroundLease>> Act)> actions = [];

        foreach (var checkIn in entry.CheckIns)
        {
            var visited = checkIn.Date.Resolve(today);
            actions.Add((visited, (_, stream) => stream.Decide(l => l.RecordStewardshipCheckIn(visited, checkIn.Notes, SeederUserId))));
        }

        if (entry.Resale is { } resale)
        {
            actions.Add((resale.Date.Resolve(today), (session, stream) =>
            {
                var lease = stream.Aggregate!;
                var templateId = lease.TemplateId
                    ?? throw new InvalidOperationException(
                        $"Ground lease seed '{entry.SeedKey}' has a resale, but its lease has no template to start the purchase from.");

                var workflowId = PurchaseWorkflowId.Generate();
                var initiated = PurchaseWorkflow.Initiate(
                    workflowId,
                    templateId,
                    lease.LandParcelId,
                    lease.UnitId,
                    Homeowner.From(resale.Buyer.FullName, resale.Buyer.Email, resale.Buyer.Phone),
                    resale.PurchasePrice,
                    SeederUserId);
                session.Events.StartStream<PurchaseWorkflow>(workflowId.Value, initiated);
            }));
        }

        if (entry.Termination is { } termination)
        {
            // Nothing happens on a lease once it has ended.
            var terminated = termination.Date.Resolve(today);
            if (actions.Any(a => a.Date >= terminated))
            {
                throw new InvalidOperationException($"Ground lease seed '{entry.SeedKey}' has a check-in on or after its termination.");
            }

            actions.Add((terminated, (_, stream) => stream.Decide(l => l.Terminate(termination.Reason, terminated, SeederUserId))));
        }

        return actions;
    }

    private static async Task<IReadOnlyList<GroundLeaseSeedEntry>> LoadEntriesAsync(CancellationToken cancellation)
    {
        var assembly = typeof(GroundLeasesSeeder).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith("ground-leases.seed.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new InvalidOperationException("Could not find embedded ground lease seed resource 'ground-leases.seed.json'.");
        }

        var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not load seed resource stream '{resourceName}'.");

        using (stream)
        {
            var entries = await JsonSerializer.DeserializeAsync<List<GroundLeaseSeedEntry>>(stream, JsonOptions, cancellation)
                .ConfigureAwait(false)
                ?? [];

            var duplicate = entries
                .GroupBy(x => x.SeedKey, StringComparer.Ordinal)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicate is not null)
            {
                throw new InvalidOperationException($"Duplicate ground lease seed key '{duplicate.Key}' in seed JSON.");
            }

            var unsourced = entries.FirstOrDefault(e => (e.PurchaseWorkflowSeedKey is null) == (e.Direct is null));
            if (unsourced is not null)
            {
                throw new InvalidOperationException($"Ground lease seed '{unsourced.SeedKey}' needs exactly one of purchaseWorkflowSeedKey and direct.");
            }

            var resoldAndTerminated = entries.FirstOrDefault(e => e.Resale is not null && e.Termination is not null);
            if (resoldAndTerminated is not null)
            {
                throw new InvalidOperationException($"Ground lease seed '{resoldAndTerminated.SeedKey}' has both a resale and a termination.");
            }

            return entries;
        }
    }

    private async Task<string> FindLeaseFromPurchaseAsync(
        IQuerySession session,
        GroundLeaseSeedEntry entry,
        CancellationToken cancellation)
    {
        var workflowLookup = await session
            .LoadAsync<PurchaseWorkflowSeedLookup>(PurchaseWorkflowSeedLookup.BuildId(entry.PurchaseWorkflowSeedKey!), cancellation)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException(
                $"Missing purchase workflow seed mapping for ground lease seed key '{entry.SeedKey}' (purchaseWorkflowSeedKey='{entry.PurchaseWorkflowSeedKey}'). Ensure PurchaseWorkflowsSeeder runs first.");
        var workflowId = workflowLookup.PurchaseWorkflowId;

        var lease = await propagation.WaitForAsync(
            $"ground lease from purchase workflow {workflowId}",
            ct => session.Query<GroundLeaseView>()
                .Where(v => v.PurchaseWorkflowId == workflowId)
                .FirstOrDefaultAsync(ct),
            cancellation).ConfigureAwait(false);

        return lease.Id;
    }

    private sealed class GroundLeaseSeedEntry
    {
        public string SeedKey { get; set; } = string.Empty;

        public string TrustSeedKey { get; set; } = string.Empty;

        public string? PurchaseWorkflowSeedKey { get; set; }

        public DirectLeaseSeedEntry? Direct { get; set; }

        public List<CheckInSeedEntry> CheckIns { get; set; } = [];

        public ResaleSeedEntry? Resale { get; set; }

        public TerminationSeedEntry? Termination { get; set; }
    }

    private sealed class DirectLeaseSeedEntry
    {
        public string LandParcelSeedKey { get; set; } = string.Empty;

        public string HomeSeedKey { get; set; } = string.Empty;

        public ContactSeedEntry Homeowner { get; set; } = new();

        public SeedDate LeaseStartDate { get; set; }

        public SeedDate RecordedOn { get; set; }

        public int TermYears { get; set; }

        public int OptionWindowDays { get; set; }

        public decimal BasePrice { get; set; }

        public decimal OriginalAppraisedValue { get; set; }

        public string CovenantTerms { get; set; } = string.Empty;

        public ResalePolicySeedEntry ResalePolicy { get; set; } = new();
    }

    private sealed class ResalePolicySeedEntry
    {
        public string FormulaKind { get; set; } = string.Empty;

        public decimal? AppreciationShare { get; set; }

        public string? IndexType { get; set; }

        public decimal? BaselineIndexValue { get; set; }

        public decimal? AnnualPercentage { get; set; }

        public decimal? ImprovementsCap { get; set; }
    }

    private class DatedSeedEntry
    {
        public SeedDate Date { get; set; }
    }

    private sealed class CheckInSeedEntry : DatedSeedEntry
    {
        public string Notes { get; set; } = string.Empty;
    }

    private sealed class ResaleSeedEntry : DatedSeedEntry
    {
        public ContactSeedEntry Buyer { get; set; } = new();

        public decimal PurchasePrice { get; set; }
    }

    private sealed class ContactSeedEntry
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }
    }

    private sealed class TerminationSeedEntry : DatedSeedEntry
    {
        public string Reason { get; set; } = string.Empty;
    }
}
