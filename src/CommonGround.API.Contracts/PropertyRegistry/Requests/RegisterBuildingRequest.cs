namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>Request to register a new building on a land parcel.</summary>
public sealed record RegisterHomeRequest(
    string LandParcelId,
    string HomeType,
    int Bedrooms,
    decimal Bathrooms,
    decimal SquareFootage,
    int YearBuilt,
    string AppraiserName,
    DateOnly InitialAssessedDate,
    decimal InitialAssessedValue)
{
}