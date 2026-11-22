using System.Text.Json.Serialization;

namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to create a ground lease directly, rather than from a purchase closing.</summary>
/// <remarks>
/// <c>FeesTrackedFrom</c> is the first day the trust tracks fees in CommonGround, on or after the lease start.
/// <c>OpeningBalance</c> is what the homeowner owed on that day, zero when paid up. Both are required: the lease
/// already exists, and a guess would hide what is owed or charge years of fees.
/// <c>MaxMortgageDebtPercent</c> is the debt cap (Art 8.3), a whole percent of the purchase option price, 1 to 200.
/// <c>InsuranceReplacementValue</c> and <c>InsuranceLiabilityPerOccurrence</c> are the insurance the lease requires
/// (Art 9.4), each zero or more; null doesn't check that amount.
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
    [property: JsonRequired] int MaxMortgageDebtPercent,
    [property: JsonRequired] DateOnly FeesTrackedFrom,
    [property: JsonRequired] decimal OpeningBalance,
    decimal LateInterestRate = 0m,
    int InterestWaiverDays = 30,
    decimal? InsuranceReplacementValue = null,
    decimal? InsuranceLiabilityPerOccurrence = null);
