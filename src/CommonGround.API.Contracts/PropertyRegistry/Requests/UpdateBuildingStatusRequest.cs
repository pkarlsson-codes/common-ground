namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>Request to update a building's status.</summary>
public sealed record UpdateBuildingStatusRequest(string NewStatus);