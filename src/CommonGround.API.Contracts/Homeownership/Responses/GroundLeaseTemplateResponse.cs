namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>Detailed response for a single ground lease template.</summary>
/// <param name="Id">Unique identifier.</param>
/// <param name="TemplateName">Display name of the template.</param>
/// <param name="TermYears">Lease term in years.</param>
/// <param name="OptionWindowDays">Days the trust has to buy a home after its homeowner gives notice to sell.</param>
/// <param name="LandUseFee">Monthly land use fee.</param>
/// <param name="RepairReserveFee">Monthly repair reserve fee.</param>
/// <param name="LateInterestRate">Yearly interest rate on a late fee, in percent; zero charges none.</param>
/// <param name="InterestWaiverDays">Days after the due date within which a payment owes no interest.</param>
/// <param name="MaxMortgageDebtPercent">Debt cap a lease from it sets, a whole percent of the purchase option price.</param>
/// <param name="InsuranceLiabilityPerOccurrence">Liability insurance per occurrence a lease from it requires, or null for none.</param>
/// <param name="CovenantTerms">Covenant terms text.</param>
/// <param name="Status">Current status (e.g. Active, Inactive).</param>
/// <param name="FormulaKind">Resale formula kind.</param>
/// <param name="AppreciationShare">Appreciation share fraction (AppraisalBased only).</param>
/// <param name="IndexType">Index type (IndexBased only).</param>
/// <param name="BaselineIndexValue">Baseline index value (IndexBased only).</param>
/// <param name="AnnualPercentage">Annual percentage (FixedPercentage only).</param>
/// <param name="ImprovementsCap">Cap on improvements value, if any.</param>
/// <param name="CreatedAt">ISO-8601 creation timestamp.</param>
/// <param name="CreatedBy">User ID of the creator.</param>
public sealed record GroundLeaseTemplateResponse(
    string Id,
    string TemplateName,
    int TermYears,
    int OptionWindowDays,
    decimal LandUseFee,
    decimal RepairReserveFee,
    decimal LateInterestRate,
    int InterestWaiverDays,
    int MaxMortgageDebtPercent,
    decimal? InsuranceLiabilityPerOccurrence,
    string CovenantTerms,
    string Status,
    string FormulaKind,
    decimal? AppreciationShare,
    string? IndexType,
    decimal? BaselineIndexValue,
    decimal? AnnualPercentage,
    decimal? ImprovementsCap,
    DateTimeOffset CreatedAt,
    string CreatedBy);
