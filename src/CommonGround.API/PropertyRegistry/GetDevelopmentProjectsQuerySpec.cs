using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.PropertyRegistry;

/// <summary>
/// Endpoint-specific query spec for listing development projects.
/// Keeps HTTP binding concerns separate from shared pagination specs.
/// </summary>
public sealed class GetDevelopmentProjectsQuerySpec : PaginationQuerySpecV2
{
    /// <summary>
    /// Initializes a new endpoint query spec from flat query-string values.
    /// </summary>
    /// <param name="pageNumber">The 1-based page number from the <c>pageNumber</c> query key.</param>
    /// <param name="pageSize">The page size from the <c>pageSize</c> query key.</param>
    /// <param name="search">Optional free-text search from the <c>search</c> query key.</param>
    /// <param name="landParcelId">Optional land parcel ID from the <c>landParcelId</c> query key.</param>
    /// <param name="homeIds">Optional home IDs from the <c>homeIds</c> query key.</param>
    public GetDevelopmentProjectsQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? landParcelId = null,
        IReadOnlyCollection<string>? homeIds = null)
        : base(pageNumber, pageSize)
    {
        Search = search;
        LandParcelId = landParcelId;
        HomeIds = homeIds;
    }

    /// <summary>
    /// Gets optional free-text search from the <c>search</c> query key.
    /// </summary>
    public string? Search { get; }

    /// <summary>
    /// Gets an optional land parcel ID from the <c>landParcelId</c> query key.
    /// </summary>
    public string? LandParcelId { get; }

    /// <summary>
    /// Gets optional home IDs from the <c>homeIds</c> query key.
    /// </summary>
    public IReadOnlyCollection<string>? HomeIds { get; }
}
