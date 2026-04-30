namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>Request to initiate a new development project for a land parcel.</summary>
public sealed record InitiateDevelopmentProjectRequest(
    string LandParcelId,
    string Description);
