using CommonGround.API.Contracts.Reports.Responses;
using CommonGround.Homeownership.Application.Queries;

namespace CommonGround.API.Reports;

/// <summary>Works out the funder report for a period from the homes' registration dates and Homeownership's leases and buybacks.</summary>
public static class FunderReportBuilder
{
    /// <summary>Builds the report.</summary>
    /// <param name="from">The first day of the period.</param>
    /// <param name="to">The last day of the period, on or after <paramref name="from"/>.</param>
    /// <param name="homesRegisteredOn">Each home's registration date in the trust's time zone.</param>
    /// <param name="records">The leases started by <paramref name="to"/> and the buybacks closed in the period.</param>
    /// <returns>The report.</returns>
    public static FunderReportResponse Build(
        DateOnly from,
        DateOnly to,
        IReadOnlyDictionary<string, DateOnly> homesRegisteredOn,
        FunderReportRecords records)
    {
        ArgumentNullException.ThrowIfNull(homesRegisteredOn);
        ArgumentNullException.ThrowIfNull(records);

        var leases = records.Leases.Where(l => l.LeaseStartDate <= to).ToList();

        // A home imported with an old lease was registered on the import day, after the lease started, so a
        // started lease also puts its home on the books.
        var homes = homesRegisteredOn.Where(h => h.Value <= to).Select(h => h.Key)
            .Concat(leases.Select(l => l.HomeId))
            .ToHashSet(StringComparer.Ordinal);
        var underLease = leases
            .Where(l => l.EndedOn is not { } ended || ended > to)
            .Select(l => l.HomeId)
            .ToHashSet(StringComparer.Ordinal);

        FunderReportSaleResponse[] sales = [.. leases
            .Where(l => l.LeaseStartDate >= from)
            .OrderBy(l => l.LeaseStartDate)
            .ThenBy(l => LabelOf(l.HomeId, l.HomeLabel), StringComparer.Ordinal)
            .Select(l => new FunderReportSaleResponse(
                l.GroundLeaseId,
                l.HomeId,
                LabelOf(l.HomeId, l.HomeLabel),
                l.LeaseStartDate,
                l.BasePrice,
                l.AppraisedValue,
                l.AppraisedValue - l.BasePrice,
                l.Eligibility?.HouseholdSize,
                l.Eligibility?.AnnualIncome,
                l.Eligibility?.BandOverPercentOfAmi,
                l.Eligibility?.BandUpToPercentOfAmi))];

        FunderReportIncomeBandResponse[] bands = [.. sales
            .GroupBy(s => (Over: s.IncomeBandOverPercentOfAmi, UpTo: s.IncomeBandUpToPercentOfAmi))
            .OrderBy(g => IsRecorded(g.Key.Over, g.Key.UpTo) ? 0 : 1)
            .ThenBy(g => g.Key.UpTo ?? int.MaxValue)
            .ThenBy(g => g.Key.Over ?? int.MinValue)
            .Select(g => new FunderReportIncomeBandResponse(g.Key.Over, g.Key.UpTo, g.Count()))];

        FunderReportResaleResponse[] resales = [.. records.Buybacks
            .Where(b => b.ClosingDate >= from && b.ClosingDate <= to)
            .OrderBy(b => b.ClosingDate)
            .ThenBy(b => LabelOf(b.HomeId, b.HomeLabel), StringComparer.Ordinal)
            .Select(b => new FunderReportResaleResponse(
                b.ResaleId,
                b.HomeId,
                LabelOf(b.HomeId, b.HomeLabel),
                b.ClosingDate,
                b.OriginalPrice,
                b.AppraisedValue,
                b.PurchaseOptionPrice,
                b.PricePaid,
                b.AppraisedValue - b.PricePaid,
                b.PricePaid - b.OriginalPrice))];

        return new FunderReportResponse(
            from,
            to,
            new FunderReportPortfolioResponse(homes.Count, underLease.Count, homes.Count - underLease.Count),
            sales,
            bands,
            resales,
            sales.Sum(s => s.AffordabilityGiven ?? 0m),
            resales.Sum(r => r.AffordabilityKept));
    }

    private static bool IsRecorded(int? over, int? upTo) => over is not null || upTo is not null;

    private static string LabelOf(string homeId, string? label) => label ?? homeId;
}
