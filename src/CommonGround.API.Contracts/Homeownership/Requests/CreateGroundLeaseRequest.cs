namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to create a ground lease directly, rather than from a purchase closing.</summary>
public sealed record CreateGroundLeaseRequest(
    string LandParcelId,
    string HomeId,
    string HomeownerFullName,
    string HomeownerEmail,
    string? HomeownerPhone,
    DateOnly LeaseStartDate,
    int TermYears,
    string CovenantTerms,
    string FormulaKind,
    decimal? AppreciationShare,
    string? IndexType,
    decimal? BaselineIndexValue,
    decimal? AnnualPercentage,
    decimal? ImprovementsCap);
