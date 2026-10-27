namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>Summary item for ground lease list responses.</summary>
public sealed record GroundLeaseListItemResponse(
    string Id,
    string LandParcelId,
    string HomeId,
    string LeaseStartDate,
    int TermYears,
    string Status,
    string FormulaKind,
    string? HomeownerFullName,
    string? HomeownerEmail,
    string CreatedAt);
