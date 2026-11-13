using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.Resales;
using Marten;

namespace CommonGround.Homeownership.Application.Queries;

/// <summary>Returns the leases and buybacks a funder report for <paramref name="From"/> to <paramref name="To"/> is built from.</summary>
/// <param name="From">The first day of the period.</param>
/// <param name="To">The last day of the period.</param>
public sealed record GetFunderReportRecords(DateOnly From, DateOnly To);

/// <summary>The leases started by the end of the period, and the buybacks that closed in it.</summary>
/// <param name="Leases">Every lease that started on or before the period's last day.</param>
/// <param name="Buybacks">Every buyback that closed in the period.</param>
public sealed record FunderReportRecords(IReadOnlyList<FunderReportLease> Leases, IReadOnlyList<FunderReportBuyback> Buybacks);

/// <summary>A ground lease, with what its purchase's Eligibility track recorded about the buyer.</summary>
/// <param name="GroundLeaseId">The lease.</param>
/// <param name="HomeId">The leased home.</param>
/// <param name="HomeLabel">The label staff know the home by, if Homeownership has it.</param>
/// <param name="LeaseStartDate">The day the lease started; for a purchase, the closing date.</param>
/// <param name="EndedOn">The day the lease ended, if it has.</param>
/// <param name="BasePrice">The price the homeowner paid.</param>
/// <param name="AppraisedValue">The home's appraised value when the lease started, if recorded.</param>
/// <param name="Eligibility">What the purchase's Eligibility track recorded, or null when the lease has no purchase or the track wasn't recorded.</param>
public sealed record FunderReportLease(
    string GroundLeaseId,
    string HomeId,
    string? HomeLabel,
    DateOnly LeaseStartDate,
    DateOnly? EndedOn,
    decimal BasePrice,
    decimal? AppraisedValue,
    FunderReportEligibility? Eligibility);

/// <summary>The buyer's verified household and income band, as recorded at eligibility verification.</summary>
/// <param name="HouseholdSize">The verified household size.</param>
/// <param name="AnnualIncome">The verified annual income.</param>
/// <param name="BandOverPercentOfAmi">The level the income is over, or null for the lowest band or when no band was recorded.</param>
/// <param name="BandUpToPercentOfAmi">The level the income is at or under, or null above the highest level or when no band was recorded.</param>
public sealed record FunderReportEligibility(
    int HouseholdSize,
    decimal AnnualIncome,
    int? BandOverPercentOfAmi,
    int? BandUpToPercentOfAmi);

/// <summary>A closed buyback and the seller's original price.</summary>
/// <param name="ResaleId">The resale.</param>
/// <param name="HomeId">The home bought back.</param>
/// <param name="HomeLabel">The label staff know the home by, if Homeownership has it.</param>
/// <param name="ClosingDate">The day the buyback closed.</param>
/// <param name="OriginalPrice">The price the seller paid, from the seller's lease.</param>
/// <param name="AppraisedValue">The latest appraisal's value.</param>
/// <param name="PurchaseOptionPrice">The purchase option price from the latest appraisal.</param>
/// <param name="PricePaid">What the trust paid at the closing.</param>
public sealed record FunderReportBuyback(
    string ResaleId,
    string HomeId,
    string? HomeLabel,
    DateOnly ClosingDate,
    decimal OriginalPrice,
    decimal AppraisedValue,
    decimal PurchaseOptionPrice,
    decimal PricePaid);

/// <summary>Handles <see cref="GetFunderReportRecords"/>.</summary>
public static class GetFunderReportRecordsHandler
{
    /// <summary>Loads the leases, their purchases' eligibility, the closed buybacks and the home labels.</summary>
    public static async Task<FunderReportRecords> Handle(
        GetFunderReportRecords request,
        IQuerySession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(session);

        var to = request.To;
        var from = request.From;

        var leases = await session.Query<GroundLeaseView>()
            .Where(l => l.LeaseStartDate <= to)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var closed = ResaleStatus.Closed.DisplayName;
        var buybacks = await session.Query<ResaleView>()
            .Where(r => r.Status == closed && r.ClosingDate >= from && r.ClosingDate <= to)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Only leases that start in the period are sales; the rest only count towards the portfolio.
        var purchaseIds = leases
            .Where(l => l.LeaseStartDate >= from && !string.IsNullOrEmpty(l.PurchaseWorkflowId))
            .Select(l => l.PurchaseWorkflowId!)
            .Distinct()
            .ToArray();
        var purchases = (await session.LoadManyAsync<PurchaseWorkflowView>(cancellationToken, purchaseIds).ConfigureAwait(false))
            .ToDictionary(p => p.Id);

        var sellerLeases = leases.ToDictionary(l => l.Id);
        var missingSellerLeaseIds = buybacks.Select(b => b.GroundLeaseId).Where(id => !sellerLeases.ContainsKey(id)).Distinct().ToArray();
        foreach (var lease in await session.LoadManyAsync<GroundLeaseView>(cancellationToken, missingSellerLeaseIds).ConfigureAwait(false))
        {
            sellerLeases[lease.Id] = lease;
        }

        var homeIds = leases.Select(l => l.HomeId).Concat(buybacks.Select(b => b.HomeId)).Distinct().ToArray();
        var labels = (await session.LoadManyAsync<HomeLabelView>(cancellationToken, homeIds).ConfigureAwait(false))
            .ToDictionary(l => l.Id, l => l.Label);

        FunderReportLease[] leaseRows = [.. leases.Select(l => new FunderReportLease(
            l.Id,
            l.HomeId,
            labels.GetValueOrDefault(l.HomeId),
            l.LeaseStartDate,
            l.EndedOn,
            l.BasePrice,
            l.OriginalAppraisedValue,
            l.PurchaseWorkflowId is { } purchaseId
                && purchases.GetValueOrDefault(purchaseId)?.EligibilityDetails is { } eligibility
                ? new FunderReportEligibility(
                    eligibility.VerifiedHouseholdSize,
                    eligibility.VerifiedAnnualIncome,
                    eligibility.IncomeBandOverPercentOfAmi,
                    eligibility.IncomeBandUpToPercentOfAmi)
                : null))];

        FunderReportBuyback[] buybackRows = [.. buybacks.Select(b => new FunderReportBuyback(
            b.Id,
            b.HomeId,
            labels.GetValueOrDefault(b.HomeId),
            b.ClosingDate!.Value,
            sellerLeases.TryGetValue(b.GroundLeaseId, out var seller)
                ? seller.BasePrice
                : throw new InvalidOperationException($"Resale {b.Id} names ground lease {b.GroundLeaseId}, which has no view."),
            b.AppraisedValue ?? throw new InvalidOperationException($"Closed resale {b.Id} has no appraised value."),
            b.PurchaseOptionPrice ?? throw new InvalidOperationException($"Closed resale {b.Id} has no purchase option price."),
            b.PricePaid ?? throw new InvalidOperationException($"Closed resale {b.Id} has no price paid.")))];

        return new FunderReportRecords(leaseRows, buybackRows);
    }
}
