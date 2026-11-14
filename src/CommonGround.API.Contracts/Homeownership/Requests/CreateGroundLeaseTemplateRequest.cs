using System.Text.Json.Serialization;

namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to create a new ground lease template.</summary>
/// <remarks>
/// <c>MaxMortgageDebtPercent</c> is the debt cap (Art 8.3) a lease from it sets, a whole percent of the purchase
/// option price, 1 to 200. <c>InsuranceLiabilityPerOccurrence</c> is the liability insurance a lease from it requires
/// (Art 9.4), zero or more; null requires none.
/// </remarks>
public sealed record CreateGroundLeaseTemplateRequest(
    string TemplateName,
    int TermYears,
    int OptionWindowDays,
    decimal LandUseFee,
    decimal RepairReserveFee,
    string CovenantTerms,
    string FormulaKind,
    decimal? AppreciationShare,
    string? IndexType,
    decimal? BaselineIndexValue,
    decimal? AnnualPercentage,
    decimal? ImprovementsCap,
    [property: JsonRequired] int MaxMortgageDebtPercent,
    decimal LateInterestRate = 0m,
    int InterestWaiverDays = 30,
    decimal? InsuranceLiabilityPerOccurrence = null);
