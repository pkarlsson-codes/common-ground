using System.Text.Json;
using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Infrastructure.Seeding;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Extensions;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using Marten;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>
/// Seeds lease fee payments on every lease the seeders before it created: each month's fee is paid a few days
/// after it falls due, so a lease reads as paid up. The leases in lease-fees.seed.json leave their last months
/// unpaid, and may pay part of the oldest of them, so the Lease Fees tab has balances to show.
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
                }

                await session.SaveChangesAsync(cancellation).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
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
    }
}
