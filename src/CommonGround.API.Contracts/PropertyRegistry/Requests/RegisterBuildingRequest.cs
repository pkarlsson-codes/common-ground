using System.Text.Json.Serialization;

namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>Request to register a new home on a land parcel.</summary>
/// <remarks>
/// <see cref="IncomeLimitPercentOfAmi"/> is the home's income limit as a percent of AMI, or null for no income
/// limit. It is required, so a client that leaves it out gets a 400 instead of a limit it didn't choose.
/// </remarks>
public sealed record RegisterHomeRequest(
    string LandParcelId,
    string HomeType,
    int Bedrooms,
    decimal Bathrooms,
    bool IsAccessible,
    [property: JsonRequired] int? IncomeLimitPercentOfAmi,
    decimal SquareFootage,
    int YearBuilt,
    string AppraiserName,
    DateOnly InitialAssessedDate,
    decimal InitialAssessedValue,
    string? UnitDesignation = null)
{
}
