namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>
/// Filter parameters for retrieving land parcels.
/// </summary>
public sealed class GetLandParcelsFilter
{
    /// <summary>
    /// Optional status filter for land parcels.
    /// </summary>
    public IReadOnlyList<string>? Statuses { get; init; }

    /// <summary>Optional free-text search across address and legal parcel number.</summary>
    public string? Search { get; init; }
}
