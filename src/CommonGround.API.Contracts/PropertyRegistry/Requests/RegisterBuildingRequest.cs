namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>Request to register a new building on a land parcel.</summary>
public sealed record RegisterBuildingRequest(
    string LandParcelId,
    string BuildingType,
    int Bedrooms,
    decimal Bathrooms,
    decimal SquareFootage,
    int YearBuilt,
    decimal InitialAssessedValue);
