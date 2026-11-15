using System.Text;
using System.Text.Json;
using CommonGround.Documents.Domain.Documents;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.HeirTransfers;
using CommonGround.Homeownership.Infrastructure.Seeding;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using Marten;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>
/// Seeds heir transfers on seeded ground leases, from JSON, written as the app does: opened on the notice date, the
/// heir recorded, and, for a completed one, both signed letters attached and the home passed to the heir. A transfer
/// with an heir and no completion is still open.
/// </summary>
public sealed class HeirTransfersSeeder(ITrustClock trustClock, PropagationWaiter propagation, IDocumentBlobStore blobs) : IDevelopmentSeeder
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
                    $"Trust seed '{entry.TrustSeedKey}' not found for heir transfer seed '{entry.SeedKey}'. Ensure TrustsSeeder runs first.");

            using var trustSession = store.LightweightSession(trustLookup.TrustId);

            var lookupId = HeirTransferSeedLookup.BuildId(entry.SeedKey);
            if (await trustSession.LoadAsync<HeirTransferSeedLookup>(lookupId, cancellation).ConfigureAwait(false) is not null)
            {
                continue;
            }

            var leaseLookup = await trustSession
                .LoadAsync<GroundLeaseSeedLookup>(GroundLeaseSeedLookup.BuildId(entry.GroundLeaseSeedKey), cancellation)
                .ConfigureAwait(false)
                ?? throw new InvalidOperationException(
                    $"Missing ground lease seed mapping for heir transfer seed '{entry.SeedKey}' (groundLeaseSeedKey='{entry.GroundLeaseSeedKey}'). Ensure GroundLeasesSeeder runs first.");

            var timeZone = await trustClock.GetTimeZoneAsync(trustLookup.TrustId, cancellation).ConfigureAwait(false);
            var today = timeZone.ToDate(TimeProviderContext.Current.UtcNow);

            await SeedClock.RunAsync(timeZone, async clock =>
            {
                var transferId = HeirTransferId.Generate();
                var noticeOn = entry.NoticeReceivedOn.Resolve(today);
                clock.MoveTo(noticeOn);

                var lease = await trustSession.Events
                    .FetchForWriting<GroundLease>(leaseLookup.GroundLeaseId, cancellation)
                    .ConfigureAwait(false);
                trustSession.Events.StartStream<HeirTransfer>(
                    transferId.Value,
                    HeirTransfer.Open(
                        transferId, lease.Aggregate!, null, entry.DiedOn.Resolve(today), noticeOn,
                        entry.NotifiedBy, entry.NotifiedByContact, entry.Notes, noticeOn, SeederUserId));
                await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);

                if (entry.Heir is { } heir)
                {
                    clock.MoveTo(heir.Date.Resolve(today));
                    var transfer = await trustSession.Events.FetchForWriting<HeirTransfer>(transferId.Value, cancellation).ConfigureAwait(false);
                    transfer.Decide(t => t.RecordHeir(
                        heir.FullName, heir.Email, heir.Phone, HeirRelationship.FromDisplayName(heir.Relationship),
                        null, null, null, null, null, SeederUserId));
                    await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
                }

                if (entry.Completed is { } completed)
                {
                    var completedOn = completed.Date.Resolve(today);
                    clock.MoveTo(completedOn);
                    var agreementId = UploadLetter(trustSession, transferId.Value, "Letter of Agreement", "heir-letter-of-agreement");
                    var acknowledgementId = UploadLetter(trustSession, transferId.Value, "Attorney's Acknowledgment", "heir-attorney-acknowledgement");
                    await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);

                    var transfer = await trustSession.Events.FetchForWriting<HeirTransfer>(transferId.Value, cancellation).ConfigureAwait(false);
                    var leaseStream = await trustSession.Events.FetchForWriting<GroundLease>(leaseLookup.GroundLeaseId, cancellation).ConfigureAwait(false);
                    transfer.Decide(t => t.Complete(completedOn, agreementId, acknowledgementId, completed.Notes, completedOn, SeederUserId));
                    leaseStream.Decide(l => l.PassToHeir(transfer.Aggregate!, completedOn, SeederUserId));
                    await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
                }

                trustSession.Store(new HeirTransferSeedLookup
                {
                    Id = lookupId,
                    SeedKey = entry.SeedKey,
                    HeirTransferId = transferId.Value,
                    SeededAt = DateTimeOffset.UtcNow,
                });

                await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
                await propagation.WaitForIdleAsync(cancellation).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }

    private static async Task<IReadOnlyList<HeirTransferSeedEntry>> LoadEntriesAsync(CancellationToken cancellation)
    {
        var assembly = typeof(HeirTransfersSeeder).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith(".heir-transfers.seed.json", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("Could not find embedded heir transfer seed resource 'heir-transfers.seed.json'.");

        var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not load seed resource stream '{resourceName}'.");

        using (stream)
        {
            var entries = await JsonSerializer.DeserializeAsync<List<HeirTransferSeedEntry>>(stream, JsonOptions, cancellation)
                .ConfigureAwait(false)
                ?? [];

            var duplicate = entries
                .GroupBy(x => x.SeedKey, StringComparer.Ordinal)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicate is not null)
            {
                throw new InvalidOperationException($"Duplicate heir transfer seed key '{duplicate.Key}' in seed JSON.");
            }

            return entries;
        }
    }

    private string UploadLetter(Marten.IDocumentSession session, string transferId, string title, string role)
    {
        var documentId = DocumentId.Generate();
        var versionId = DocumentVersionId.Generate();
        var blobKey = $"documents/{documentId.Value}/{versionId.Value}.txt";
        using var content = new MemoryStream(Encoding.UTF8.GetBytes($"{title}, signed (seeded sample file)."));
        var uploaded = blobs.UploadAsync(content, "text/plain", blobKey, CancellationToken.None).GetAwaiter().GetResult();

        var events = Document.Upload(
            documentId,
            versionId,
            $"{title} signed.txt",
            SensitivityLevel.Confidential,
            RetentionClass.LongTerm7yr,
            uploaded.BlobKey,
            "signed.txt",
            "text/plain",
            uploaded.SizeBytes,
            uploaded.Sha256,
            SeederUserId,
            ("HeirTransfer", transferId, role));
        session.Events.StartStream<Document>(documentId.Value, [.. events]);
        return documentId.Value;
    }

    private sealed class HeirTransferSeedEntry
    {
        public string SeedKey { get; set; } = string.Empty;

        public string TrustSeedKey { get; set; } = string.Empty;

        public string GroundLeaseSeedKey { get; set; } = string.Empty;

        public SeedDate DiedOn { get; set; }

        public SeedDate NoticeReceivedOn { get; set; }

        public string NotifiedBy { get; set; } = string.Empty;

        public string? NotifiedByContact { get; set; }

        public string? Notes { get; set; }

        public HeirSeedEntry? Heir { get; set; }

        public CompletedSeedEntry? Completed { get; set; }
    }

    private sealed class HeirSeedEntry
    {
        public SeedDate Date { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string Relationship { get; set; } = string.Empty;
    }

    private sealed class CompletedSeedEntry
    {
        public SeedDate Date { get; set; }

        public string? Notes { get; set; }
    }
}
