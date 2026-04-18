namespace CommonGround.API.Contracts.PropertyRegistry.Responses;

/// <summary>Response DTO for a building list item.</summary>
public sealed record BuildingListItemResponse(
    string Id,
    string LandParcelId,
    string BuildingType,
    int Bedrooms,
    decimal Bathrooms,
    decimal SquareFootage,
    decimal AssessedValue,
    string Status);