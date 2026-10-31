using System.Text.Json;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.HomeBuyers.Infrastructure.Seeding;
using CommonGround.SharedKernel.Domain;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using Marten;
using Marten.Schema;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>
/// Seeds development households, their readiness and their applications from JSON, so the
/// waiting list and home selections have candidates to work with.
/// </summary>
public sealed class HouseholdsInitialData : IInitialData
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly UserId SeederUserId = new("user-seeder-system");

    /// <inheritdoc/>
    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
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
                    $"Trust seed '{entry.TrustSeedKey}' not found for household seed '{entry.SeedKey}'. Ensure TrustsInitialData runs first.");

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

            var household = Household.Register(
                [.. entry.Members.Select(m => HouseholdMember.Create(m.Name, m.Email, m.Phone, m.IsPrimary))],
                HouseholdCircumstances.Create(entry.RequiresAccessibility, entry.AnnualIncome),
                SeederUserId);

            if (entry.HousingCounseling is { } counseling)
            {
                household.RecordHousingCounseling(
                    counseling.CertificationNumber, counseling.CompletedDate, counseling.CounselorOrganization, SeederUserId);
            }

            if (entry.CltOrientationCompleted is { } orientationDate)
            {
                household.RecordCltOrientation(orientationDate, SeederUserId);
            }

            if (entry.Prequalification is { } prequalification)
            {
                household.RecordMortgagePrequalification(
                    prequalification.LenderName, prequalification.ApprovedAmount, prequalification.ExpiryDate, SeederUserId);
            }

            trustSession.Events.StartStream<Household>(household.Id.Value, household.UncommittedEvents.ToArray());
            household.UncommittedEvents.MarkAsCommitted();

            string? applicationId = null;
            if (entry.Application is { } applicationEntry)
            {
                var application = HomeownerApplication.Submit(household.Id, household.Declare(), null, SeederUserId);
                if (applicationEntry.ApprovedDate is { } approvedDate)
                {
                    application.Approve(approvedDate, null, SeederUserId);
                }

                trustSession.Events.StartStream<HomeownerApplication>(application.Id.Value, application.UncommittedEvents.ToArray());
                application.UncommittedEvents.MarkAsCommitted();
                applicationId = application.Id.Value;
            }

            trustSession.Store(new HouseholdSeedLookup
            {
                Id = lookupId,
                SeedKey = entry.SeedKey,
                HouseholdId = household.Id.Value,
                ApplicationId = applicationId,
                SeededAt = DateTimeOffset.UtcNow,
            });

            await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
        }
    }

    private static async Task<IReadOnlyList<HouseholdSeedEntry>> LoadEntriesAsync(CancellationToken cancellation)
    {
        var assembly = typeof(HouseholdsInitialData).Assembly;
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

        public DateOnly? CltOrientationCompleted { get; set; }

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

        public DateOnly CompletedDate { get; set; }

        public string CounselorOrganization { get; set; } = string.Empty;
    }

    private sealed class PrequalificationSeedEntry
    {
        public string LenderName { get; set; } = string.Empty;

        public decimal ApprovedAmount { get; set; }

        public DateOnly ExpiryDate { get; set; }
    }

    private sealed class ApplicationSeedEntry
    {
        public DateOnly? ApprovedDate { get; set; }
    }
}
