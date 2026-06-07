using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>Query string parameters for the get development projects endpoint.</summary>
public sealed class GetDevelopmentProjectsQuerySpec : PaginationQuerySpecV2
{
    /// <summary>Initializes a new instance with optional filters.</summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="search">Optional free-text search.</param>
    /// <param name="landParcelId">Optional land parcel ID filter.</param>
    /// <param name="homeIds">Optional home ID filters.</param>
    public GetDevelopmentProjectsQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? landParcelId = null,
        IReadOnlyList<string>? homeIds = null)
        : base(pageNumber, pageSize)
    {
        Search = search;
        LandParcelId = landParcelId;
        HomeIds = homeIds;
    }

    /// <summary>Gets optional free-text search.</summary>
    public string? Search { get; }

    /// <summary>Gets an optional land parcel ID filter.</summary>
    public string? LandParcelId { get; }

    /// <summary>Gets optional home ID filters.</summary>
    public IReadOnlyList<string>? HomeIds { get; }

    /// <inheritdoc cref="PaginationQuerySpecV2.AppendFilterParts"/>
    protected override void AppendFilterParts(List<string> parts)
    {
        AddIfNotWhiteSpace(parts, "search", Search);
        AddIfNotWhiteSpace(parts, "landParcelId", LandParcelId);
        AddEach(parts, "homeIds", HomeIds);
    }
}
