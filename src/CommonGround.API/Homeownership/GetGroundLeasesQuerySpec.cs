using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Homeownership;

/// <summary>
/// Endpoint-specific query spec for listing ground leases.
/// Keeps HTTP binding concerns separate from shared pagination specs.
/// </summary>
public sealed class GetGroundLeasesQuerySpec : PaginationQuerySpecV2
{
    /// <summary>
    /// Initializes a new endpoint query spec from flat query-string values.
    /// </summary>
    /// <param name="pageNumber">The 1-based page number from the <c>pageNumber</c> query key.</param>
    /// <param name="pageSize">The page size from the <c>pageSize</c> query key.</param>
    /// <param name="statuses">Optional status filters from the <c>statuses</c> query key.</param>
    /// <param name="landParcelId">Optional land parcel ID from the <c>landParcelId</c> query key.</param>
    /// <param name="homeId">Optional home ID from the <c>homeId</c> query key.</param>
    /// <param name="search">Optional free-text search from the <c>search</c> query key.</param>
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

    /// <summary>
    /// Gets optional status filters from the <c>statuses</c> query key.
    /// </summary>
    public IReadOnlyList<string>? Statuses { get; }

    /// <summary>
    /// Gets an optional land parcel ID from the <c>landParcelId</c> query key.
    /// </summary>
    public string? LandParcelId { get; }

    /// <summary>
    /// Gets an optional home ID from the <c>homeId</c> query key.
    /// </summary>
    public string? HomeId { get; }

    /// <summary>
    /// Gets optional free-text search from the <c>search</c> query key.
    /// </summary>
    public string? Search { get; }
}