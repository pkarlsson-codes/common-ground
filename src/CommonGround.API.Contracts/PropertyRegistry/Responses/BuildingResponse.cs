namespace CommonGround.API.Contracts.PropertyRegistry.Responses;

/// <summary>Response DTO for a home detail view.</summary>
public sealed record HomeResponse(
    string Id,
    string LandParcelId,
    string Label,
    string? UnitDesignation,
    string HomeType,
    int Bedrooms,
    decimal Bathrooms,
    bool IsAccessible,
    decimal SquareFootage,
    int YearBuilt,
    decimal AssessedValue,
    DateOnly? LastAssessedDate,
    string? ConditionNotes,
    DateTimeOffset RegisteredAt,
    DateOnly? AvailableAgainDate,
    DateOnly? SelectionOpenedDate,
    DateOnly? UnderContractDate,
    DateOnly? LeasedDate,
    DateOnly? ResaleNoticeDate,
    string Occupancy,
    string SaleStage);
