namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Filter parameters for retrieving ground leases.</summary>
public sealed class GetGroundLeasesFilter
{
    /// <summary>Optional status filter.</summary>
    public IReadOnlyList<string>? Statuses { get; init; }

    /// <summary>Optional land parcel ID filter.</summary>
    public string? LandParcelId { get; init; }

    /// <summary>Optional building ID filter.</summary>
    public string? BuildingId { get; init; }
}
