using System.Text.Json;
using CommonGround.HomeBuyers.Infrastructure.Seeding;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Infrastructure.Seeding;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using Marten;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>
/// Seeds progress on the purchase workflows that home selections started, from JSON: the steps
/// recorded so far, and a scheduled closing, a stall, a failure or a withdrawal. A closed purchase
/// propagates on to its ground lease and stewardship case. A purchase that fails or is withdrawn
/// propagates back, returning the household to the waiting list and the home to selection.
/// </summary>
public sealed class PurchaseWorkflowsSeeder(ITrustClock trustClock, PropagationWaiter propagation) : IDevelopmentSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly UserId SeederUserId = UserId.Create("user-seeder-system");

    private static readonly string[] TerminalPhaseNames =
    [
        WorkflowPhase.Closed.DisplayName,
        WorkflowPhase.Failed.DisplayName,
        WorkflowPhase.Withdrawn.DisplayName,
    ];

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
                    $"Trust seed '{entry.TrustSeedKey}' not found for purchase workflow seed '{entry.SeedKey}'. Ensure TrustsSeeder runs first.");

            using var trustSession = store.LightweightSession(trustLookup.TrustId);

            // The workflow stream belongs to the selection that started it, so the lookup alone marks
            // its steps as seeded.
            var lookupId = PurchaseWorkflowSeedLookup.BuildId(entry.SeedKey);
            var lookup = await trustSession.LoadAsync<PurchaseWorkflowSeedLookup>(lookupId, cancellation).ConfigureAwait(false);
            if (lookup is not null)
            {
                continue;
            }

            var selectionLookup = await trustSession
                .LoadAsync<HomeSelectionSeedLookup>(HomeSelectionSeedLookup.BuildId(entry.HomeSelectionSeedKey), cancellation)
                .ConfigureAwait(false);
            var applicationId = selectionLookup?.SelectedApplicationId
                ?? throw new InvalidOperationException(
                    $"No selected application for purchase workflow seed '{entry.SeedKey}' (homeSelectionSeedKey='{entry.HomeSelectionSeedKey}'). Ensure HomeSelectionsSeeder runs first and the selection chose someone.");

            var workflow = await propagation.WaitForAsync(
                $"purchase workflow for application {applicationId} to start",
                ct => trustSession.Query<PurchaseWorkflowView>()
                    .Where(v => v.ApplicationId == applicationId && !TerminalPhaseNames.Contains(v.Phase))
                    .FirstOrDefaultAsync(ct),
                cancellation).ConfigureAwait(false);

            var timeZone = await trustClock.GetTimeZoneAsync(trustLookup.TrustId, cancellation).ConfigureAwait(false);
            var today = timeZone.ToDate(TimeProviderContext.Current.UtcNow);

            await SeedClock.RunAsync(timeZone, async clock =>
            {
                var stream = await trustSession.Events
                    .FetchForWriting<PurchaseWorkflow>(workflow.Id, cancellation)
                    .ConfigureAwait(false);

                // Each step is recorded on its own date, in date order, so the purchase reads as having
                // moved on then.
                foreach (var (date, decide) in Steps(entry, today).OrderBy(s => s.Date))
                {
                    clock.MoveTo(date);
                    stream.Decide(decide);
                }

                trustSession.Store(new PurchaseWorkflowSeedLookup
                {
                    Id = lookupId,
                    SeedKey = entry.SeedKey,
                    PurchaseWorkflowId = workflow.Id,
                    SeededAt = DateTimeOffset.UtcNow,
                });

                await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
                await propagation.WaitForIdleAsync(cancellation).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }

    private static List<(DateOnly Date, Func<PurchaseWorkflow, object?> Decide)> Steps(PurchaseWorkflowSeedEntry entry, DateOnly today)
    {
        List<(DateOnly Date, Func<PurchaseWorkflow, object?> Decide)> steps = [];

        if (entry.Eligibility is { } eligibility)
        {
            var verified = eligibility.Date.Resolve(today);

            // Seeded purchases all start from a selection, so the documents confirm what the household declared.
            steps.Add((verified, w => w.RecordEligibilityVerified(
                verified,
                w.DeclaredAnnualIncome ?? throw new InvalidOperationException($"Purchase workflow {w.Id} has no declared income to verify."),
                w.DeclaredHouseholdSize ?? throw new InvalidOperationException($"Purchase workflow {w.Id} has no declared household size to verify."),
                eligibility.Notes,
                SeederUserId)));
        }

        if (entry.Appraisal is { } appraisal)
        {
            var appraised = appraisal.Date.Resolve(today);
            steps.Add((appraised, w => w.RecordAppraisalCompleted(
                appraisal.AppraisedValue, appraised, appraisal.AppraiserName, appraisal.Notes, SeederUserId)));
        }

        if (entry.Financing is { } financing)
        {
            steps.Add((financing.Date.Resolve(today), w => w.RecordFinancingCommitted(
                financing.LenderName, financing.CommittedAmount, financing.PreapprovalLetterDate.Resolve(today), SeederUserId)));
        }

        if (entry.Inspection is { } inspection)
        {
            var inspected = inspection.Date.Resolve(today);
            steps.Add((inspected, w => w.RecordInspectionCompleted(inspected, inspection.InspectorName, inspection.Notes, SeederUserId)));
        }

        if (entry.Title is { } title)
        {
            var cleared = title.Date.Resolve(today);
            steps.Add((cleared, w => w.RecordTitleCleared(cleared, title.TitleCompanyName, SeederUserId)));
        }

        if (entry.LeaseSigning is { } leaseSigning)
        {
            var signed = leaseSigning.Date.Resolve(today);
            steps.Add((signed, w => w.RecordLeaseExecuted(signed, SeederUserId)));
        }

        if (entry.ClosingScheduled is { } closing)
        {
            steps.Add((closing.Date.Resolve(today), w => w.ScheduleClosing(closing.ClosingDate.Resolve(today), SeederUserId)));
        }

        if (entry.Closed is { } closed)
        {
            var closedOn = closed.Date.Resolve(today);
            steps.Add((closedOn, w => w.RecordPurchaseClosed(closedOn, closed.Notes, SeederUserId)));
        }

        if (entry.Stall is { } stall)
        {
            var stalled = stall.Date.Resolve(today);
            steps.Add((stalled, w => w.StallWorkflow(stall.Reason, stalled, stall.ResolutionTargetDate.Resolve(today), SeederUserId)));
        }

        if (entry.Failure is { } failure)
        {
            var failed = failure.Date.Resolve(today);
            steps.Add((failed, w => w.FailPurchase(failure.Reason, failed, SeederUserId)));
        }

        if (entry.Withdrawal is { } withdrawal)
        {
            var withdrawn = withdrawal.Date.Resolve(today);
            steps.Add((withdrawn, w => w.WithdrawPurchase(withdrawn, withdrawal.Reason, SeederUserId)));
        }

        return steps;
    }

    private static async Task<IReadOnlyList<PurchaseWorkflowSeedEntry>> LoadEntriesAsync(CancellationToken cancellation)
    {
        var assembly = typeof(PurchaseWorkflowsSeeder).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith("purchase-workflows.seed.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new InvalidOperationException("Could not find embedded purchase workflow seed resource 'purchase-workflows.seed.json'.");
        }

        var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not load seed resource stream '{resourceName}'.");

        using (stream)
        {
            var entries = await JsonSerializer.DeserializeAsync<List<PurchaseWorkflowSeedEntry>>(stream, JsonOptions, cancellation)
                .ConfigureAwait(false)
                ?? [];

            var duplicate = entries
                .GroupBy(x => x.SeedKey, StringComparer.Ordinal)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicate is not null)
            {
                throw new InvalidOperationException($"Duplicate purchase workflow seed key '{duplicate.Key}' in seed JSON.");
            }

            var outcomes = entries.FirstOrDefault(e => new object?[] { e.ClosingScheduled, e.Stall, e.Failure, e.Withdrawal }.Count(o => o is not null) > 1);
            if (outcomes is not null)
            {
                throw new InvalidOperationException($"Purchase workflow seed '{outcomes.SeedKey}' has more than one of closingScheduled, stall, failure and withdrawal.");
            }

            var unscheduled = entries.FirstOrDefault(e => e.Closed is not null && e.ClosingScheduled is null);
            if (unscheduled is not null)
            {
                throw new InvalidOperationException($"Purchase workflow seed '{unscheduled.SeedKey}' is closed without a scheduled closing.");
            }

            return entries;
        }
    }

    private sealed class PurchaseWorkflowSeedEntry
    {
        public string SeedKey { get; set; } = string.Empty;

        public string TrustSeedKey { get; set; } = string.Empty;

        public string HomeSelectionSeedKey { get; set; } = string.Empty;

        public EligibilitySeedEntry? Eligibility { get; set; }

        public AppraisalSeedEntry? Appraisal { get; set; }

        public FinancingSeedEntry? Financing { get; set; }

        public InspectionSeedEntry? Inspection { get; set; }

        public TitleSeedEntry? Title { get; set; }

        public DatedSeedEntry? LeaseSigning { get; set; }

        public ClosingScheduledSeedEntry? ClosingScheduled { get; set; }

        public ClosedSeedEntry? Closed { get; set; }

        public StallSeedEntry? Stall { get; set; }

        public ReasonSeedEntry? Failure { get; set; }

        public ReasonSeedEntry? Withdrawal { get; set; }
    }

    private class DatedSeedEntry
    {
        public SeedDate Date { get; set; }
    }

    private sealed class EligibilitySeedEntry : DatedSeedEntry
    {
        public string? Notes { get; set; }
    }

    private sealed class AppraisalSeedEntry : DatedSeedEntry
    {
        public decimal AppraisedValue { get; set; }

        public string AppraiserName { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }

    private sealed class FinancingSeedEntry : DatedSeedEntry
    {
        public string LenderName { get; set; } = string.Empty;

        public decimal CommittedAmount { get; set; }

        public SeedDate PreapprovalLetterDate { get; set; }
    }

    private sealed class InspectionSeedEntry : DatedSeedEntry
    {
        public string InspectorName { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }

    private sealed class TitleSeedEntry : DatedSeedEntry
    {
        public string? TitleCompanyName { get; set; }
    }

    private sealed class ClosingScheduledSeedEntry : DatedSeedEntry
    {
        public SeedDate ClosingDate { get; set; }
    }

    private sealed class ClosedSeedEntry : DatedSeedEntry
    {
        public string? Notes { get; set; }
    }

    private sealed class StallSeedEntry : DatedSeedEntry
    {
        public string Reason { get; set; } = string.Empty;

        public SeedDate ResolutionTargetDate { get; set; }
    }

    private sealed class ReasonSeedEntry : DatedSeedEntry
    {
        public string Reason { get; set; } = string.Empty;
    }
}
