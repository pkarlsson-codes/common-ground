namespace CommonGround.API.Contracts.PropertyRegistry.Responses;

/// <summary>Response DTO for a land parcel detail view.</summary>
public sealed record LandParcelResponse(
    string Id,
    string LegalParcelNumber,
    string Address,
    decimal AreaAcres,
    DateOnly AcquisitionDate,
    string AcquisitionMethod,
    string? CovenantTerms,
    bool IsDecommissioned,
    DateTimeOffset RegisteredAt,
    string RegisteredBy);
