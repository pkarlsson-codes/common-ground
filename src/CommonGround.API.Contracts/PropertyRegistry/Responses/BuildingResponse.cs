namespace CommonGround.API.Contracts.PropertyRegistry.Responses;

/// <summary>Response DTO for a home detail view.</summary>
public sealed record HomeResponse(
    string Id,
    string LandParcelId,
    string HomeType,
    int Bedrooms,
    decimal Bathrooms,
    decimal SquareFootage,
    int YearBuilt,
    decimal AssessedValue,
    DateOnly? LastAssessedDate,
    string? ConditionNotes,
    string Status,
    DateTimeOffset RegisteredAt);
