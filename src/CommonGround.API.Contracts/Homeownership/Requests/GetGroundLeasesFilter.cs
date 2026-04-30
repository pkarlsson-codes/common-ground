namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Filter parameters for retrieving ground leases.</summary>
public sealed class GetGroundLeasesFilter
{
    /// <summary>Optional status filter.</summary>
    public IReadOnlyList<string>? Statuses { get; init; }

    /// <summary>Optional land parcel ID filter.</summary>
    public string? LandParcelId { get; init; }

    /// <summary>Optional home ID filter.</summary>
    public string? HomeId { get; init; }

    /// <summary>Optional free-text search across homeowner name and email.</summary>
    public string? Search { get; init; }
}
