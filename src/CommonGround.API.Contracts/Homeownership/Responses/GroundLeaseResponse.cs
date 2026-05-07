namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>Detail response for a ground lease.</summary>
public sealed record GroundLeaseResponse(
    string Id,
    string LandParcelId,
    string HomeId,
    string LeaseStartDate,
    int TermYears,
    string CovenantTerms,
    string Status,
    string FormulaKind,
    decimal? AppreciationShare,
    string? IndexType,
    decimal? BaselineIndexValue,
    decimal? AnnualPercentage,
    decimal? ImprovementsCap,
    string? HomeownerFullName,
    string? HomeownerEmail,
    string? HomeownerPhone,
    IReadOnlyList<StewardshipCheckInDto> StewardshipCheckIns,
    string CreatedAt,
    string CreatedBy);

/// <summary>DTO for a stewardship check-in entry.</summary>
public sealed record StewardshipCheckInDto(
    string CheckInDate,
    string Notes,
    string RecordedBy,
    string OccurredAt);
