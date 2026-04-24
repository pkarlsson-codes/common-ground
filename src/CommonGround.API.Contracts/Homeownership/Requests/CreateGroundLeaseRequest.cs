namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to create a new ground lease.</summary>
public sealed record CreateGroundLeaseRequest(
    string LandParcelId,
    string BuildingId,
    string LeaseStartDate,
    int TermYears,
    string CovenantTerms,
    string FormulaKind,
    decimal? AppreciationShare,
    string? IndexType,
    decimal? BaselineIndexValue,
    decimal? AnnualPercentage,
    decimal? ImprovementsCap);
