namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>Detailed response for a single ground lease template.</summary>
/// <param name="Id">Unique identifier.</param>
/// <param name="TemplateName">Display name of the template.</param>
/// <param name="TermYears">Lease term in years.</param>
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
    string CovenantTerms,
    string Status,
    string FormulaKind,
    decimal? AppreciationShare,
    string? IndexType,
    decimal? BaselineIndexValue,
    decimal? AnnualPercentage,
    decimal? ImprovementsCap,
    string CreatedAt,
    string CreatedBy);
