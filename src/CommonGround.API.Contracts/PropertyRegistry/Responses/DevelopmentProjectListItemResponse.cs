namespace CommonGround.API.Contracts.PropertyRegistry.Responses;

/// <summary>Response DTO for a development project list item.</summary>
public sealed record DevelopmentProjectListItemResponse(
    string Id,
    string LandParcelId,
    string Description,
    string Status);
