namespace CommonGround.API.Contracts.PropertyRegistry.Responses;

/// <summary>Response DTO for a land parcel list item.</summary>
public sealed record LandParcelListItemResponse(
    string Id,
    string LegalParcelNumber,
    string Address,
    decimal AreaAcres,
    string Status,
    DateTimeOffset RegisteredAt);
