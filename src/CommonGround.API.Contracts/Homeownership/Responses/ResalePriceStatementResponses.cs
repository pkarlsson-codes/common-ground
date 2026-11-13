namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>A lease's resale price on a statement date, with the figures used.</summary>
/// <param name="StatementDate">The day the price is worked out on.</param>
/// <param name="YearsOwned">Full years from the lease start to the statement date.</param>
/// <param name="MarketValue">The market value used; null unless the formula is appraisal based.</param>
/// <param name="MarketValueSource">Where the market value came from.</param>
/// <param name="IndexValue">The index value used; null unless the formula is index based.</param>
/// <param name="ImprovementsCredit">The improvements credit counted, after the cap; null for none.</param>
/// <param name="Price">The most the home may sell for on the statement date.</param>
public sealed record ResalePriceStatementPriceResponse(
    DateOnly StatementDate,
    int YearsOwned,
    decimal? MarketValue,
    string? MarketValueSource,
    decimal? IndexValue,
    decimal? ImprovementsCredit,
    decimal Price);

/// <summary>The leases on the yearly resale price statement cycle.</summary>
/// <param name="Today">The trust's today, the default statement date.</param>
/// <param name="LetterSignOff">The trust's sign-off for letters; empty for none.</param>
/// <param name="Leases">The leases, soonest due first.</param>
public sealed record ResalePriceStatementsDueResponse(
    DateOnly Today,
    string LetterSignOff,
    IReadOnlyList<ResalePriceStatementLeaseResponse> Leases);

/// <summary>A lease on the yearly statement cycle, with what its statement letter and price need.</summary>
/// <param name="GroundLeaseId">The lease.</param>
/// <param name="HomeownerFullName">The homeowner's name.</param>
/// <param name="HomeId">The home.</param>
/// <param name="HomeLabel">The home's label, if known.</param>
/// <param name="LeaseStartDate">The lease start.</param>
/// <param name="BasePrice">What the homeowner paid, which the formula starts from.</param>
/// <param name="OriginalAppraisedValue">The appraised value at purchase, if known.</param>
/// <param name="FormulaKind">AppraisalBased, IndexBased or FixedPercentage.</param>
/// <param name="AppreciationShare">The homeowner's share of appreciation (appraisal based).</param>
/// <param name="IndexType">The index (index based).</param>
/// <param name="BaselineIndexValue">The index value at purchase (index based).</param>
/// <param name="AnnualPercentage">The yearly rate (fixed rate).</param>
/// <param name="ImprovementsCap">The most an improvements credit counts for, if the lease credits any.</param>
/// <param name="LastStatementDate">The latest statement's date, or null when none was sent.</param>
/// <param name="NextDueOn">When the next statement is due.</param>
public sealed record ResalePriceStatementLeaseResponse(
    string GroundLeaseId,
    string? HomeownerFullName,
    string HomeId,
    string? HomeLabel,
    DateOnly LeaseStartDate,
    decimal BasePrice,
    decimal? OriginalAppraisedValue,
    string FormulaKind,
    decimal? AppreciationShare,
    string? IndexType,
    decimal? BaselineIndexValue,
    decimal? AnnualPercentage,
    decimal? ImprovementsCap,
    DateOnly? LastStatementDate,
    DateOnly NextDueOn);
