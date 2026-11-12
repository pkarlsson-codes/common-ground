namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>Request to register a new home on a land parcel.</summary>
/// <remarks>
/// <see cref="IncomeLimitPercentOfAmi"/> is the home's income limit as a percent of AMI. Left out, it is 80;
/// sent as null, the home has no income limit.
/// </remarks>
public sealed record RegisterHomeRequest(
    string LandParcelId,
    string HomeType,
    int Bedrooms,
    decimal Bathrooms,
    bool IsAccessible,
    decimal SquareFootage,
    int YearBuilt,
    string AppraiserName,
    DateOnly InitialAssessedDate,
    decimal InitialAssessedValue,
    string? UnitDesignation = null,
    int? IncomeLimitPercentOfAmi = 80)
{
}