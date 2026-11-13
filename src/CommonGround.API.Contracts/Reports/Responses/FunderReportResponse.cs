namespace CommonGround.API.Contracts.Reports.Responses;

/// <summary>What the trust held on the last day of a period, whom it sold to in the period, and what it bought back.</summary>
/// <param name="From">The first day of the period.</param>
/// <param name="To">The last day of the period.</param>
/// <param name="Portfolio">The homes on the books on <paramref name="To"/>.</param>
/// <param name="Sales">One row per ground lease that started in the period, earliest first.</param>
/// <param name="IncomeBands">How many sales fell in each income band, lowest band first and "not recorded" last.</param>
/// <param name="Resales">One row per buyback that closed in the period, earliest first.</param>
/// <param name="TotalAffordabilityGiven">The sum of every sale's affordability given that has an appraised value.</param>
/// <param name="TotalAffordabilityKept">The sum of every buyback's affordability kept.</param>
public sealed record FunderReportResponse(
    DateOnly From,
    DateOnly To,
    FunderReportPortfolioResponse Portfolio,
    IReadOnlyList<FunderReportSaleResponse> Sales,
    IReadOnlyList<FunderReportIncomeBandResponse> IncomeBands,
    IReadOnlyList<FunderReportResaleResponse> Resales,
    decimal TotalAffordabilityGiven,
    decimal TotalAffordabilityKept);

/// <summary>The homes on the books on the period's last day.</summary>
/// <param name="Homes">Homes registered, or under a ground lease that had started, by that day.</param>
/// <param name="UnderGroundLease">Homes under a ground lease on that day.</param>
/// <param name="HeldOrForSale">The rest: homes the trust held or was selling on that day.</param>
public sealed record FunderReportPortfolioResponse(int Homes, int UnderGroundLease, int HeldOrForSale);

/// <summary>A ground lease that started in the period.</summary>
/// <param name="GroundLeaseId">The lease.</param>
/// <param name="HomeId">The leased home.</param>
/// <param name="HomeLabel">The label staff know the home by.</param>
/// <param name="LeaseStartDate">The day the lease started; for a purchase, the closing date.</param>
/// <param name="Price">The price the homeowner paid (the lease's base price).</param>
/// <param name="AppraisedValue">The home's appraised value at the sale, or null when not recorded.</param>
/// <param name="AffordabilityGiven">Appraised value minus price, or null when the appraised value isn't recorded.</param>
/// <param name="HouseholdSize">The verified household size, or null when the lease has no recorded eligibility verification.</param>
/// <param name="AnnualIncome">The verified annual income, or null when the lease has no recorded eligibility verification.</param>
/// <param name="IncomeBandOverPercentOfAmi">The level the income is over, or null for the lowest band or when no band was recorded.</param>
/// <param name="IncomeBandUpToPercentOfAmi">The level the income is at or under, or null above the highest level or when no band was recorded.</param>
public sealed record FunderReportSaleResponse(
    string GroundLeaseId,
    string HomeId,
    string HomeLabel,
    DateOnly LeaseStartDate,
    decimal Price,
    decimal? AppraisedValue,
    decimal? AffordabilityGiven,
    int? HouseholdSize,
    decimal? AnnualIncome,
    int? IncomeBandOverPercentOfAmi,
    int? IncomeBandUpToPercentOfAmi);

/// <summary>How many sales fell in one income band. Both levels null means the band was not recorded.</summary>
/// <param name="OverPercentOfAmi">The level the band starts above, or null for the lowest band.</param>
/// <param name="UpToPercentOfAmi">The level the band goes up to, or null above the highest level.</param>
/// <param name="Count">How many sales fell in it.</param>
public sealed record FunderReportIncomeBandResponse(int? OverPercentOfAmi, int? UpToPercentOfAmi, int Count);

/// <summary>A buyback that closed in the period.</summary>
/// <param name="ResaleId">The resale.</param>
/// <param name="HomeId">The home bought back.</param>
/// <param name="HomeLabel">The label staff know the home by.</param>
/// <param name="ClosingDate">The day the buyback closed.</param>
/// <param name="OriginalPrice">The price the seller paid.</param>
/// <param name="AppraisedValue">The home's appraised value at the resale.</param>
/// <param name="PurchaseOptionPrice">What the lease's formula let the trust pay.</param>
/// <param name="PricePaid">What the trust paid.</param>
/// <param name="AffordabilityKept">Appraised value minus price paid.</param>
/// <param name="SellerGain">Price paid minus original price.</param>
public sealed record FunderReportResaleResponse(
    string ResaleId,
    string HomeId,
    string HomeLabel,
    DateOnly ClosingDate,
    decimal OriginalPrice,
    decimal AppraisedValue,
    decimal PurchaseOptionPrice,
    decimal PricePaid,
    decimal AffordabilityKept,
    decimal SellerGain);
