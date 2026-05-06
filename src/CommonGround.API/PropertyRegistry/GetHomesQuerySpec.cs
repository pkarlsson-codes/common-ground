using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.PropertyRegistry;

/// <summary>
/// Endpoint-specific query spec for listing homes.
/// Keeps HTTP binding concerns separate from shared pagination specs.
/// </summary>
public sealed class GetHomesQuerySpec : PaginationQuerySpecV2
{
    /// <summary>
    /// Initializes a new endpoint query spec from flat query-string values.
    /// </summary>
    /// <param name="pageNumber">The 1-based page number from the <c>pageNumber</c> query key.</param>
    /// <param name="pageSize">The page size from the <c>pageSize</c> query key.</param>
    /// <param name="landParcelIds">Optional land parcel IDs from the <c>landParcelIds</c> query key.</param>
    /// <param name="statuses">Optional status filters from the <c>statuses</c> query key.</param>
    public GetHomesQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        IReadOnlyList<string>? landParcelIds = null,
        IReadOnlyList<string>? statuses = null)
        : base(pageNumber, pageSize)
    {
        LandParcelIds = landParcelIds;
        Statuses = statuses;
    }

    /// <summary>
    /// Gets optional land parcel IDs from the <c>landParcelIds</c> query key.
    /// </summary>
    public IReadOnlyList<string>? LandParcelIds { get; }

    /// <summary>
    /// Gets optional status filters from the <c>statuses</c> query key.
    /// </summary>
    public IReadOnlyList<string>? Statuses { get; }
}
