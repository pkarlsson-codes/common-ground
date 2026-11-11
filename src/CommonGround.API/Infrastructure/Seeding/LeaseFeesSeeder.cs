using System.Text.Json;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.LeaseFeeNoticePolicies;
using CommonGround.Homeownership.Infrastructure.Seeding;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.TrustManagement.Application.Queries;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using Marten;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>
/// Seeds lease fee payments on every lease the seeders before it created: each month's fee is paid a few days
/// after it falls due, so a lease reads as paid up. The leases in lease-fees.seed.json leave their last months
/// unpaid, and may pay part of the oldest of them, so the Lease Fees tab has balances to show; one of them also has
/// a first late notice sent. Each trust's notice policy keeps the default days and gets a sign-off with its name.
/// </summary>
public sealed class LeaseFeesSeeder(ITrustClock trustClock) : IDevelopmentSeeder
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
        var trusts = await defaultSession.Query<TrustSeedLookup>().ToListAsync(cancellation).ConfigureAwait(false);

        foreach (var trust in trusts)
        {
            using var session = store.LightweightSession(trust.TrustId);

            var arrears = new Dictionary<string, LeaseFeeSeedEntry>(StringComparer.Ordinal);
            foreach (var entry in entries.Where(e => e.TrustSeedKey == trust.SeedKey))
            {
                var lookup = await session
                    .LoadAsync<GroundLeaseSeedLookup>(GroundLeaseSeedLookup.BuildId(entry.GroundLeaseSeedKey), cancellation)
                    .ConfigureAwait(false)
                    ?? throw new InvalidOperationException(
                        $"Lease fee seed names ground lease seed '{entry.GroundLeaseSeedKey}', which wasn't seeded. Ensure GroundLeasesSeeder runs first.");
                arrears[lookup.GroundLeaseId] = entry;
            }

            var timeZone = await trustClock.GetTimeZoneAsync(trust.TrustId, cancellation).ConfigureAwait(false);
            await SeedPolicyAsync(store, session, trust.TrustId, cancellation).ConfigureAwait(false);
            var today = timeZone.ToDate(TimeProviderContext.Current.UtcNow);
            var leaseIds = await session.Query<GroundLeaseView>().Select(v => v.Id).ToListAsync(cancellation).ConfigureAwait(false);

            await SeedClock.RunAsync(timeZone, async clock =>
            {
                foreach (var leaseId in leaseIds)
                {
                    var stream = await session.Events.FetchForWriting<GroundLease>(leaseId, cancellation).ConfigureAwait(false);
                    var lease = stream.Aggregate!;
                    if (lease.LeaseFeePayments.Count > 0)
                    {
                        continue;
                    }

                    foreach (var (received, amount, method, reference) in Payments(lease, arrears.GetValueOrDefault(leaseId), today))
                    {
                        clock.MoveTo(received);
                        stream.Decide(l => l.RecordLeaseFeePayment(
                            LeaseFeePaymentId.Generate(), received, amount, method, reference, received, SeederUserId));
                    }

                    // stream.Aggregate doesn't hold the payments appended above; Decide hands over the lease that does.
                    DateOnly? oldestUnpaid = null;
                    stream.Decide(l =>
                    {
                        oldestUnpaid = l.LeaseFeeStatementAsOf(today).OldestUnpaidDueDate;
                        return null;
                    });

                    if (arrears.GetValueOrDefault(leaseId)?.FirstNoticeAfterDays is { } noticeDays
                        && oldestUnpaid?.AddDays(noticeDays) is { } sentOn
                        && sentOn <= today)
                    {
                        clock.MoveTo(sentOn);
                        stream.Decide(l => l.RecordLeaseFeeNotice(
                            LeaseFeeNoticeId.Generate(), LeaseFeeNoticeLevel.First, sentOn, NoticeDelivery.FirstClassMail, sentOn, SeederUserId));
                    }
                }

                await session.SaveChangesAsync(cancellation).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }

    private static async Task SeedPolicyAsync(IDocumentStore store, IDocumentSession session, string trustId, CancellationToken cancellation)
    {
        var id = LeaseFeeNoticePolicyId.ForTrust(trustId);
        var stream = await session.Events.FetchForWriting<LeaseFeeNoticePolicy>(id.Value, cancellation).ConfigureAwait(false);
        if (stream.Aggregate is not null)
        {
            return;
        }

        var trust = await GetTrustHandler.Handle(new GetTrust(trustId), store, cancellation).ConfigureAwait(false);
        var signOff = $"{trust?.Name ?? "Community Land Trust"}\nStewardship Team\n(503) 555-0100 · stewardship@example.org";
        stream.AppendOne(LeaseFeeNoticePolicy.Set(id, LeaseFeeNoticeSchedule.Default, signOff, SeederUserId));
    }

    private static List<(DateOnly Received, decimal Amount, PaymentMethod Method, string? Reference)> Payments(
        GroundLease lease, LeaseFeeSeedEntry? arrears, DateOnly today)
    {
        var statement = lease.LeaseFeeStatementAsOf(today);
        var monthlyFee = statement.MonthlyFee;
        var unpaid = Math.Min(arrears?.MonthsBehind ?? 0, statement.ChargeCount);
        var paidMonths = statement.ChargeCount - unpaid;
        var firstDue = LeaseFeeStatement.FirstDueDateOnOrAfter(lease.LeaseStartDate);

        // Each lease pays one way, so the history reads like one household's habit.
        var byCheck = (lease.CurrentHomeowner?.FullName.Value.Length ?? 0) % 2 == 0;
        var lastDay = lease.EndedOn is { } ended && ended < today ? ended : today;

        List<(DateOnly, decimal, PaymentMethod, string?)> payments = [];
        for (var month = 0; month < paidMonths; month++)
        {
            var due = firstDue.AddMonths(month);
            var received = Min(due.AddDays(1 + (month % 5)), lastDay);
            payments.Add(byCheck
                ? (received, monthlyFee, PaymentMethod.Check, $"Check #{1001 + month}")
                : (received, monthlyFee, PaymentMethod.BankTransfer, null));
        }

        if (arrears?.PartialPayment is { } partial && unpaid > 0)
        {
            var due = firstDue.AddMonths(paidMonths);
            payments.Add((Min(due.AddDays(3), lastDay), partial, PaymentMethod.Cash, null));
        }

        return payments;
    }

    private static DateOnly Min(DateOnly a, DateOnly b) => a < b ? a : b;

    private static async Task<IReadOnlyList<LeaseFeeSeedEntry>> LoadEntriesAsync(CancellationToken cancellation)
    {
        var assembly = typeof(LeaseFeesSeeder).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith("lease-fees.seed.json", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("Could not find embedded lease fee seed resource 'lease-fees.seed.json'.");

        var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not load seed resource stream '{resourceName}'.");

        using (stream)
        {
            return await JsonSerializer.DeserializeAsync<List<LeaseFeeSeedEntry>>(stream, JsonOptions, cancellation)
                .ConfigureAwait(false)
                ?? [];
        }
    }

    private sealed class LeaseFeeSeedEntry
    {
        public string TrustSeedKey { get; set; } = string.Empty;

        public string GroundLeaseSeedKey { get; set; } = string.Empty;

        public int MonthsBehind { get; set; }

        public decimal? PartialPayment { get; set; }

        public int? FirstNoticeAfterDays { get; set; }
    }
}
