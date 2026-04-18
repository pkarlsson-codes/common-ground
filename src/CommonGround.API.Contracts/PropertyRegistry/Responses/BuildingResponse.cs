namespace CommonGround.API.Contracts.PropertyRegistry.Responses;

/// <summary>Response DTO for a building detail view.</summary>
public sealed record BuildingResponse(
    string Id,
    string LandParcelId,
    string BuildingType,
    int Bedrooms,
    decimal Bathrooms,
    decimal SquareFootage,
    int YearBuilt,
    decimal AssessedValue,
    DateOnly? LastAssessedDate,
    string? ConditionNotes,
    string Status,
    DateTimeOffset RegisteredAt);
