using System.Text.Json;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.HomeBuyers.Infrastructure.Seeding;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Services;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using Marten;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>
/// Seeds development households, their readiness and their applications from JSON, so the
/// waiting list and home selections have candidates to work with.
/// </summary>
public sealed class HouseholdsSeeder(ITrustClock trustClock, PropagationWaiter propagation) : IDevelopmentSeeder
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
                    $"Trust seed '{entry.TrustSeedKey}' not found for household seed '{entry.SeedKey}'. Ensure TrustsSeeder runs first.");

            using var trustSession = store.LightweightSession(trustLookup.TrustId);

            var lookupId = HouseholdSeedLookup.BuildId(entry.SeedKey);
            var lookup = await trustSession.LoadAsync<HouseholdSeedLookup>(lookupId, cancellation).ConfigureAwait(false);
            if (lookup is not null)
            {
                var streamState = await trustSession.Events.FetchStreamStateAsync(lookup.HouseholdId, cancellation).ConfigureAwait(false);
                if (streamState is not null)
                {
                    continue;
                }
            }

            var timeZone = await trustClock.GetTimeZoneAsync(trustLookup.TrustId, cancellation).ConfigureAwait(false);
            var today = timeZone.ToDate(TimeProviderContext.Current.UtcNow);

            // Each step is raised on its own date, so registration, readiness and the application
            // read as having happened then.
            await SeedClock.RunAsync(timeZone, async clock =>
            {
                var householdId = HouseholdId.Generate();
                clock.MoveTo(entry.RegisteredDate.Resolve(today));
                var registered = Household.Register(
                    householdId,
                    [.. entry.Members.Select(m => HouseholdMember.Create(PersonName.Create(m.Name), EmailAddress.Create(m.Email), new PhoneNumber(m.Phone), m.IsPrimary))],
                    HouseholdCircumstances.Create(entry.RequiresAccessibility, Money.Create(entry.AnnualIncome)),
                    SeederUserId);

                // Readiness doesn't change the declaration, so each decision is taken on the registered household.
                var household = Household.Create(registered);
                List<(DateOnly Date, Func<object?> Decide)> readiness = [];
                if (entry.HousingCounseling is { } counseling)
                {
                    var completed = counseling.CompletedDate.Resolve(today);
                    readiness.Add((completed, () => household.RecordHousingCounseling(
                        counseling.CertificationNumber, completed, counseling.CounselorOrganization, SeederUserId)));
                }

                if (entry.CltOrientationCompleted is { } orientation)
                {
                    var completed = orientation.Resolve(today);
                    readiness.Add((completed, () => household.RecordCltOrientation(completed, SeederUserId)));
                }

                if (entry.Prequalification is { } prequalification)
                {
                    readiness.Add((prequalification.RecordedDate.Resolve(today), () => household.RecordMortgagePrequalification(
                        prequalification.LenderName, prequalification.ApprovedAmount, prequalification.ExpiryDate.Resolve(today), SeederUserId)));
                }

                List<object> householdEvents = [registered];
                foreach (var (date, decide) in readiness.OrderBy(r => r.Date))
                {
                    clock.MoveTo(date);
                    if (decide() is { } decided)
                    {
                        householdEvents.Add(decided);
                    }
                }

                trustSession.Events.StartStream<Household>(householdId.Value, householdEvents);

                string? applicationId = null;
                if (entry.Application is { } applicationEntry)
                {
                    var id = HomeownerApplicationId.Generate();
                    clock.MoveTo(applicationEntry.SubmittedDate.Resolve(today));
                    var submitted = HomeownerApplication.Submit(id, householdId, household.Declare(), null, SeederUserId);
                    var application = HomeownerApplication.Create(submitted);
                    List<object> applicationEvents = [submitted];
                    if (applicationEntry.ApprovedDate is { } approved)
                    {
                        var approvedDate = approved.Resolve(today);
                        clock.MoveTo(approvedDate);
                        var approval = application.Approve(approvedDate, null, SeederUserId);
                        application.Apply(approval);
                        applicationEvents.Add(approval);
                    }

                    if (applicationEntry.Denial is { } denial)
                    {
                        var deniedDate = denial.Date.Resolve(today);
                        clock.MoveTo(deniedDate);
                        applicationEvents.Add(application.DenyApplication(denial.Reason, deniedDate, SeederUserId));
                    }
                    else if (applicationEntry.Withdrawal is { } withdrawal)
                    {
                        var withdrawnDate = withdrawal.Date.Resolve(today);
                        clock.MoveTo(withdrawnDate);
                        applicationEvents.Add(application.Withdraw(withdrawnDate, withdrawal.Reason, SeederUserId));
                    }

                    trustSession.Events.StartStream<HomeownerApplication>(id.Value, applicationEvents);
                    applicationId = id.Value;
                }

                trustSession.Store(new HouseholdSeedLookup
                {
                    Id = lookupId,
                    SeedKey = entry.SeedKey,
                    HouseholdId = householdId.Value,
                    ApplicationId = applicationId,
                    SeededAt = DateTimeOffset.UtcNow,
                });

                await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
                await propagation.WaitForIdleAsync(cancellation).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }

    private static async Task<IReadOnlyList<HouseholdSeedEntry>> LoadEntriesAsync(CancellationToken cancellation)
    {
        var assembly = typeof(HouseholdsSeeder).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith("households.seed.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new InvalidOperationException("Could not find embedded household seed resource 'households.seed.json'.");
        }

        var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not load seed resource stream '{resourceName}'.");

        using (stream)
        {
            var entries = await JsonSerializer.DeserializeAsync<List<HouseholdSeedEntry>>(stream, JsonOptions, cancellation)
                .ConfigureAwait(false)
                ?? [];

            var duplicate = entries
                .GroupBy(x => x.SeedKey, StringComparer.Ordinal)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicate is not null)
            {
                throw new InvalidOperationException($"Duplicate household seed key '{duplicate.Key}' in seed JSON.");
            }

            return entries;
        }
    }

    private sealed class HouseholdSeedEntry
    {
        public string SeedKey { get; set; } = string.Empty;

        public string TrustSeedKey { get; set; } = string.Empty;

        public decimal AnnualIncome { get; set; }

        public bool RequiresAccessibility { get; set; }

        public List<MemberSeedEntry> Members { get; set; } = [];

        public HousingCounselingSeedEntry? HousingCounseling { get; set; }

        public SeedDate RegisteredDate { get; set; }

        public SeedDate? CltOrientationCompleted { get; set; }

        public PrequalificationSeedEntry? Prequalification { get; set; }

        public ApplicationSeedEntry? Application { get; set; }
    }

    private sealed class MemberSeedEntry
    {
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }
    }

    private sealed class HousingCounselingSeedEntry
    {
        public string CertificationNumber { get; set; } = string.Empty;

        public SeedDate CompletedDate { get; set; }

        public string CounselorOrganization { get; set; } = string.Empty;
    }

    private sealed class PrequalificationSeedEntry
    {
        public string LenderName { get; set; } = string.Empty;

        public decimal ApprovedAmount { get; set; }

        public SeedDate RecordedDate { get; set; }

        public SeedDate ExpiryDate { get; set; }
    }

    private sealed class ApplicationSeedEntry
    {
        public SeedDate SubmittedDate { get; set; }

        public SeedDate? ApprovedDate { get; set; }

        public ApplicationOutcomeSeedEntry? Denial { get; set; }

        public ApplicationOutcomeSeedEntry? Withdrawal { get; set; }
    }

    private sealed class ApplicationOutcomeSeedEntry
    {
        public SeedDate Date { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}
