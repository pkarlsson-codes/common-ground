using System.Text.Json;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Infrastructure.Seeding;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Infrastructure.Seeding;
using CommonGround.TrustManagement.Infrastructure.Seeding;
using Marten;

namespace CommonGround.API.Infrastructure.Seeding;

/// <summary>
/// Seeds development ground lease templates from JSON, one per resale formula, so ground
/// leases and purchase workflows have templates to pick from.
/// </summary>
public sealed class GroundLeaseTemplatesSeeder : IDevelopmentSeeder
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
                    $"Trust seed '{entry.TrustSeedKey}' not found for ground lease template seed '{entry.SeedKey}'. Ensure TrustsSeeder runs first.");

            using var trustSession = store.LightweightSession(trustLookup.TrustId);

            var lookupId = GroundLeaseTemplateSeedLookup.BuildId(entry.SeedKey);
            var lookup = await trustSession.LoadAsync<GroundLeaseTemplateSeedLookup>(lookupId, cancellation).ConfigureAwait(false);
            if (lookup is not null)
            {
                var streamState = await trustSession.Events.FetchStreamStateAsync(lookup.TemplateId, cancellation).ConfigureAwait(false);
                if (streamState is not null)
                {
                    continue;
                }
            }

            var templateId = GroundLeaseTemplateId.Generate();
            var defined = GroundLeaseTemplate.Define(
                templateId,
                entry.TemplateName,
                entry.TermYears,
                entry.OptionWindowDays,
                entry.CovenantTerms,
                ToResalePolicyTerms(entry),
                LeaseFeeTerms.Create(entry.LandUseFee, entry.RepairReserveFee),
                SeederUserId);

            object[] events = entry.Archived
                ? [defined, GroundLeaseTemplate.Create(defined).Archive(SeederUserId)]
                : [defined];
            trustSession.Events.StartStream<GroundLeaseTemplate>(templateId.Value, events);

            trustSession.Store(new GroundLeaseTemplateSeedLookup
            {
                Id = lookupId,
                SeedKey = entry.SeedKey,
                TemplateId = templateId.Value,
                SeededAt = DateTimeOffset.UtcNow,
            });

            await trustSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
        }
    }

    private static ResalePolicyTerms ToResalePolicyTerms(GroundLeaseTemplateSeedEntry entry)
    {
        var resale = entry.ResalePolicy;
        return ResalePolicyTerms.FromString(
            resale.FormulaKind,
            resale.AppreciationShare,
            resale.IndexType,
            resale.BaselineIndexValue,
            resale.AnnualPercentage,
            resale.ImprovementsCap);
    }

    private static async Task<IReadOnlyList<GroundLeaseTemplateSeedEntry>> LoadEntriesAsync(CancellationToken cancellation)
    {
        var assembly = typeof(GroundLeaseTemplatesSeeder).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith("ground-lease-templates.seed.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new InvalidOperationException("Could not find embedded ground lease template seed resource 'ground-lease-templates.seed.json'.");
        }

        var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not load seed resource stream '{resourceName}'.");

        using (stream)
        {
            var entries = await JsonSerializer.DeserializeAsync<List<GroundLeaseTemplateSeedEntry>>(stream, JsonOptions, cancellation)
                .ConfigureAwait(false)
                ?? [];

            var duplicate = entries
                .GroupBy(x => x.SeedKey, StringComparer.Ordinal)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicate is not null)
            {
                throw new InvalidOperationException($"Duplicate ground lease template seed key '{duplicate.Key}' in seed JSON.");
            }

            return entries;
        }
    }

    private sealed class GroundLeaseTemplateSeedEntry
    {
        public string SeedKey { get; set; } = string.Empty;

        public string TrustSeedKey { get; set; } = string.Empty;

        public string TemplateName { get; set; } = string.Empty;

        public int TermYears { get; set; }

        public int OptionWindowDays { get; set; }

        public decimal LandUseFee { get; set; }

        public decimal RepairReserveFee { get; set; }

        public string CovenantTerms { get; set; } = string.Empty;

        public ResalePolicySeedEntry ResalePolicy { get; set; } = new();

        public bool Archived { get; set; }
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
}
