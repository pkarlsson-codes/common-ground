namespace CommonGround.API.Contracts.PropertyRegistry.Responses;

/// <summary>Response DTO for a single development project.</summary>
/// <param name="Id">The project ID.</param>
/// <param name="LandParcelId">The associated land parcel ID.</param>
/// <param name="Name">The project name.</param>
/// <param name="Description">The project description.</param>
/// <param name="Status">The current project status.</param>
/// <param name="ProjectType">The project type: <c>NewConstruction</c> or <c>Rehabilitation</c>.</param>
/// <param name="HomeIds">The homes included in the project; empty for new construction.</param>
public sealed record DevelopmentProjectResponse(
    string Id,
    string LandParcelId,
    string Name,
    string? Description,
    string Status,
    string ProjectType,
    IReadOnlyCollection<string> HomeIds);
