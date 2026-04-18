namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>
/// Filter parameters for retrieving buildings.
/// </summary>
public sealed class GetBuildingsFilter
{
    /// <summary>
    /// Optional filter for land parcel IDs.
    /// </summary>
    public IReadOnlyList<string>? LandParcelIds { get; init; }

    /// <summary>
    /// Optional status filter for buildings.
    /// </summary>
    public IReadOnlyList<string>? Statuses { get; init; }
}