namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>Request to initiate a new development project for a land parcel.</summary>
/// <param name="LandParcelId">The land parcel to associate the project with.</param>
/// <param name="Name">The name of the development project.</param>
/// <param name="Type">The project type discriminator: <c>NewConstruction</c> or <c>Rehabilitation</c>.</param>
/// <param name="Description">An optional description of the development project.</param>
/// <param name="ExistingHomes">Home IDs to include; required for Rehabilitation projects.</param>
public sealed record InitiateDevelopmentProjectRequest(
    string LandParcelId,
    string Name,
    string Type,
    string? Description,
    List<string>? ExistingHomes);
