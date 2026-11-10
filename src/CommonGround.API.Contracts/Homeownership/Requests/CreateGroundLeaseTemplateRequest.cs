namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to create a new ground lease template.</summary>
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
    decimal? ImprovementsCap);
