using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.PropertyRegistry;

/// <summary>
/// Endpoint-specific query spec for listing home assessments.
/// Keeps HTTP binding concerns separate from shared pagination specs.
/// </summary>
public sealed class GetHomeAssessmentsQuerySpec : PaginationQuerySpecV2
{
    /// <summary>
    /// Initializes a new endpoint query spec from flat query-string values.
    /// </summary>
    /// <param name="pageNumber">The 1-based page number from the <c>pageNumber</c> query key.</param>
    /// <param name="pageSize">The page size from the <c>pageSize</c> query key.</param>
    public GetHomeAssessmentsQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize)
        : base(pageNumber, pageSize)
    {
    }
}
