using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Homeownership;

/// <summary>
/// Endpoint-specific query spec for listing purchase workflows.
/// Keeps HTTP binding concerns separate from shared pagination specs.
/// </summary>
public sealed class GetPurchaseWorkflowsQuerySpec : PaginationQuerySpecV2
{
    /// <summary>
    /// Initializes a new endpoint query spec from flat query-string values.
    /// </summary>
    /// <param name="pageNumber">The 1-based page number from the <c>pageNumber</c> query key.</param>
    /// <param name="pageSize">The page size from the <c>pageSize</c> query key.</param>
    /// <param name="groundLeaseId">Optional ground lease ID from the <c>groundLeaseId</c> query key.</param>
    /// <param name="phase">Optional workflow phase from the <c>phase</c> query key.</param>
    /// <param name="search">Optional free-text search from the <c>search</c> query key.</param>
    public GetPurchaseWorkflowsQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        string? groundLeaseId = null,
        string? phase = null,
        string? search = null)
        : base(pageNumber, pageSize)
    {
        GroundLeaseId = groundLeaseId;
        Phase = phase;
        Search = search;
    }

    /// <summary>
    /// Gets an optional ground lease ID from the <c>groundLeaseId</c> query key.
    /// </summary>
    public string? GroundLeaseId { get; }

    /// <summary>
    /// Gets an optional workflow phase from the <c>phase</c> query key.
    /// </summary>
    public string? Phase { get; }

    /// <summary>
    /// Gets optional free-text search from the <c>search</c> query key.
    /// </summary>
    public string? Search { get; }
}
