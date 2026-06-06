using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Query string parameters for the get ground leases endpoint.</summary>
public sealed class GetGroundLeasesQuerySpec : PaginationQuerySpecV2
{
    /// <summary>Initializes a new instance with optional filters.</summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="statuses">Optional status filters.</param>
    /// <param name="landParcelId">Optional land parcel ID filter.</param>
    /// <param name="homeId">Optional home ID filter.</param>
    /// <param name="search">Optional free-text search across homeowner name and email.</param>
    public GetGroundLeasesQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        IReadOnlyList<string>? statuses = null,
        string? landParcelId = null,
        string? homeId = null,
        string? search = null)
        : base(pageNumber, pageSize)
    {
        Statuses = statuses;
        LandParcelId = landParcelId;
        HomeId = homeId;
        Search = search;
    }

    /// <summary>Gets optional status filters.</summary>
    public IReadOnlyList<string>? Statuses { get; }

    /// <summary>Gets an optional land parcel ID filter.</summary>
    public string? LandParcelId { get; }

    /// <summary>Gets an optional home ID filter.</summary>
    public string? HomeId { get; }

    /// <summary>Gets optional free-text search.</summary>
    public string? Search { get; }

    /// <inheritdoc cref="PaginationQuerySpecV2.AppendFilterParts"/>
    protected override void AppendFilterParts(List<string> parts)
    {
        AddEach(parts, "statuses", Statuses);
        AddIfNotWhiteSpace(parts, "landParcelId", LandParcelId);
        AddIfNotWhiteSpace(parts, "homeId", HomeId);
        AddIfNotWhiteSpace(parts, "search", Search);
    }
}
