namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>
/// Filter parameters for retrieving development projects.
/// </summary>
public sealed class GetDevelopmentProjectsFilter
{
    /// <summary>Optional free-text search across name and description.</summary>
    public string? Search { get; init; }

    /// <summary>Optional land parcel ID filter.</summary>
    public string? LandParcelId { get; init; }

    /// <summary>Optional list of home IDs to filter by.</summary>
    public IReadOnlyCollection<string>? HomeIds { get; init; }
}
