namespace CommonGround.API.Contracts.PropertyRegistry.Responses;

/// <summary>Response DTO for a home list item.</summary>
public sealed record HomeListItemResponse(
    string Id,
    string LandParcelId,
    string Label,
    string? UnitDesignation,
    string HomeType,
    int Bedrooms,
    decimal Bathrooms,
    decimal SquareFootage,
    decimal AssessedValue,
    string Status,
    string Occupancy,
    string SaleStage);