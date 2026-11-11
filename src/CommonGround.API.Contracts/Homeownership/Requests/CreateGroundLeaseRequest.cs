namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to create a ground lease directly, rather than from a purchase closing.</summary>
/// <remarks>
/// <c>FeesTrackedFrom</c> is the first day the trust tracks fees in CommonGround, on or after the lease start; null
/// tracks from the lease start. <c>OpeningBalance</c> is what the homeowner owed on that day; null is zero.
/// </remarks>
public sealed record CreateGroundLeaseRequest(
    string LandParcelId,
    string HomeId,
    string HomeownerFullName,
    string HomeownerEmail,
    string? HomeownerPhone,
    DateOnly LeaseStartDate,
    int TermYears,
    int OptionWindowDays,
    decimal LandUseFee,
    decimal RepairReserveFee,
    decimal BasePrice,
    decimal OriginalAppraisedValue,
    string CovenantTerms,
    string FormulaKind,
    decimal? AppreciationShare,
    string? IndexType,
    decimal? BaselineIndexValue,
    decimal? AnnualPercentage,
    decimal? ImprovementsCap,
    decimal LateInterestRate = 0m,
    int InterestWaiverDays = 30,
    DateOnly? FeesTrackedFrom = null,
    decimal? OpeningBalance = null);
