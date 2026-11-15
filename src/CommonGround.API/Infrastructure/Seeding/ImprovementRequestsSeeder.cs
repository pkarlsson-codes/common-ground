using System.Text;
using System.Text.Json;
using CommonGround.Documents.Domain.Documents;
using CommonGround.Homeownership.Application.Queries;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.ImprovementRequests;
using CommonGround.Homeownership.Infrastructure.Seeding;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using Marten;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>
/// Seeds improvement requests on seeded ground leases, from JSON: each received on its own date, then approved,
/// denied, completed with a receipt, or withdrawn, written as the app does. A request with no later step is still
/// waiting; one received more than two weeks ago shows as overdue on Today.
/// </summary>
public sealed class ImprovementRequestsSeeder(ITrustClock trustClock, PropagationWaiter propagation, IDocumentBlobStore blobs) : IDevelopmentSeeder
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
                    $"Trust seed '{entry.TrustSeedKey}' not found for improvement request seed '{entry.SeedKey}'. Ensure TrustsSeeder runs first.");

            using var trustSession = store.LightweightSession(trustLookup.TrustId);

            var lookupId = ImprovementRequestSeedLookup.BuildId(entry.SeedKey);
            if (await trustSession.LoadAsync<ImprovementRequestSeedLookup>(lookupId, cancellation).ConfigureAwait(false) is not null)
            {
                continue;
            }

            var leaseLookup = await trustSession
                .LoadAsync<GroundLeaseSeedLookup>(GroundLeaseSeedLookup.BuildId(entry.GroundLeaseSeedKey), cancellation)
                .ConfigureAwait(false)
                ?? throw new InvalidOperationException(
                    $"Missing ground lease seed mapping for improvement request seed '{entry.SeedKey}' (groundLeaseSeedKey='{entry.GroundLeaseSeedKey}'). Ensure GroundLeasesSeeder runs first.");

            var timeZone = await trustClock.GetTimeZoneAsync(trustLookup.TrustId, cancellation).ConfigureAwait(false);
            var today = timeZone.ToDate(TimeProviderContext.Current.UtcNow);

            await SeedClock.RunAsync(timeZone, async clock =>
            {
                var requestId = ImprovementRequestId.Generate();
                var receivedOn = entry.ReceivedOn.Resolve(today);
                clock.MoveTo(receivedOn);

                var lease = await trustSession.Events
                    .FetchForWriting<GroundLease>(leaseLookup.GroundLeaseId, cancellation)
                    .ConfigureAwait(false);
                trustSession.Events.StartStream<ImprovementRequest>(
                    requestId.Value,
                    ImprovementRequest.Receive(
                        requestId, lease.Aggregate!, receivedOn, entry.Description, Money.Create(entry.EstimatedCost), receivedOn, SeederUserId));
                await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);

                if (entry.Approved is { } approved)
                {
                    var approvedOn = approved.Date.Resolve(today);
                    clock.MoveTo(approvedOn);
                    var request = await trustSession.Events.FetchForWriting<ImprovementRequest>(requestId.Value, cancellation).ConfigureAwait(false);
                    var cap = lease.Aggregate!.ResalePolicyTerms.ImprovementsCap is { } amount ? Money.Create(amount) : null;
                    var creditOnLease = await ConfirmedImprovementCredit
                        .ForLeaseAsync(trustSession, leaseLookup.GroundLeaseId, cancellation).ConfigureAwait(false);
                    request.Decide(r => r.Approve(
                        approvedOn, Money.Create(approved.PreApprovedCredit), cap, creditOnLease, approved.Notes, approvedOn, SeederUserId));
                    await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
                }

                if (entry.Completed is { } completed)
                {
                    var completedOn = completed.Date.Resolve(today);
                    clock.MoveTo(completedOn);
                    var receiptId = await UploadReceiptAsync(trustSession, requestId.Value, cancellation).ConfigureAwait(false);
                    var request = await trustSession.Events.FetchForWriting<ImprovementRequest>(requestId.Value, cancellation).ConfigureAwait(false);
                    request.Decide(r => r.RecordCompleted(
                        completedOn, Money.Create(completed.ActualCost), [receiptId], Money.Create(completed.ConfirmedCredit),
                        completed.Notes, completedOn, SeederUserId));
                    await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
                }

                if (entry.Denied is { } denied)
                {
                    var deniedOn = denied.Date.Resolve(today);
                    clock.MoveTo(deniedOn);
                    var request = await trustSession.Events.FetchForWriting<ImprovementRequest>(requestId.Value, cancellation).ConfigureAwait(false);
                    request.Decide(r => r.Deny(deniedOn, denied.Reason, deniedOn, SeederUserId));
                    await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
                }

                if (entry.Withdrawn is { } withdrawn)
                {
                    var withdrawnOn = withdrawn.Date.Resolve(today);
                    clock.MoveTo(withdrawnOn);
                    var request = await trustSession.Events.FetchForWriting<ImprovementRequest>(requestId.Value, cancellation).ConfigureAwait(false);
                    request.Decide(r => r.Withdraw(withdrawnOn, withdrawn.Reason, withdrawnOn, SeederUserId));
                    await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
                }

                trustSession.Store(new ImprovementRequestSeedLookup
                {
                    Id = lookupId,
                    SeedKey = entry.SeedKey,
                    ImprovementRequestId = requestId.Value,
                    SeededAt = DateTimeOffset.UtcNow,
                });

                await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
                await propagation.WaitForIdleAsync(cancellation).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }

    private static async Task<IReadOnlyList<ImprovementRequestSeedEntry>> LoadEntriesAsync(CancellationToken cancellation)
    {
        var assembly = typeof(ImprovementRequestsSeeder).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith(".improvement-requests.seed.json", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("Could not find embedded improvement request seed resource 'improvement-requests.seed.json'.");

        var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not load seed resource stream '{resourceName}'.");

        using (stream)
        {
            var entries = await JsonSerializer.DeserializeAsync<List<ImprovementRequestSeedEntry>>(stream, JsonOptions, cancellation)
                .ConfigureAwait(false)
                ?? [];

            var duplicate = entries
                .GroupBy(x => x.SeedKey, StringComparer.Ordinal)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicate is not null)
            {
                throw new InvalidOperationException($"Duplicate improvement request seed key '{duplicate.Key}' in seed JSON.");
            }

            return entries;
        }
    }

    private async Task<string> UploadReceiptAsync(Marten.IDocumentSession session, string requestId, CancellationToken cancellation)
    {
        var documentId = DocumentId.Generate();
        var versionId = DocumentVersionId.Generate();
        var blobKey = $"documents/{documentId.Value}/{versionId.Value}.txt";
        using var content = new MemoryStream(Encoding.UTF8.GetBytes("Contractor invoice and receipts (seeded sample file)."));
        var uploaded = await blobs.UploadAsync(content, "text/plain", blobKey, cancellation).ConfigureAwait(false);

        var events = Document.Upload(
            documentId,
            versionId,
            "Improvement receipts receipts.txt",
            SensitivityLevel.Confidential,
            RetentionClass.LongTerm7yr,
            uploaded.BlobKey,
            "receipts.txt",
            "text/plain",
            uploaded.SizeBytes,
            uploaded.Sha256,
            SeederUserId,
            ("ImprovementRequest", requestId, "improvement-receipt"));
        session.Events.StartStream<Document>(documentId.Value, [.. events]);
        return documentId.Value;
    }

    private sealed class ImprovementRequestSeedEntry
    {
        public string SeedKey { get; set; } = string.Empty;

        public string TrustSeedKey { get; set; } = string.Empty;

        public string GroundLeaseSeedKey { get; set; } = string.Empty;

        public SeedDate ReceivedOn { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal EstimatedCost { get; set; }

        public ApprovedSeedEntry? Approved { get; set; }

        public CompletedSeedEntry? Completed { get; set; }

        public DeniedSeedEntry? Denied { get; set; }

        public WithdrawnSeedEntry? Withdrawn { get; set; }
    }

    private sealed class ApprovedSeedEntry
    {
        public SeedDate Date { get; set; }

        public decimal PreApprovedCredit { get; set; }

        public string? Notes { get; set; }
    }

    private sealed class CompletedSeedEntry
    {
        public SeedDate Date { get; set; }

        public decimal ActualCost { get; set; }

        public decimal ConfirmedCredit { get; set; }

        public string? Notes { get; set; }
    }

    private sealed class DeniedSeedEntry
    {
        public SeedDate Date { get; set; }

        public string Reason { get; set; } = string.Empty;
    }

    private sealed class WithdrawnSeedEntry
    {
        public SeedDate Date { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}
