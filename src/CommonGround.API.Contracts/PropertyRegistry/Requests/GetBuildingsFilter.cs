namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>
/// Filter parameters for retrieving buildings.
/// </summary>
public sealed class GetHomesFilter
{
    /// <summary>
    /// Optional filter for land parcel IDs.
    /// </summary>
    public IReadOnlyList<string>? LandParcelIds { get; init; }

    /// <summary>
    /// Optional status filter.
    /// </summary>
    public IReadOnlyList<string>? Statuses { get; init; }
}