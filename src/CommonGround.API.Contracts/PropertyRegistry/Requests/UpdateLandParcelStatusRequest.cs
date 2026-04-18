namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>Request to update a land parcel's status.</summary>
public sealed record UpdateLandParcelStatusRequest(string NewStatus);