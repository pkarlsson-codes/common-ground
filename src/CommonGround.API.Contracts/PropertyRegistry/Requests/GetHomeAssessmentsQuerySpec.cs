using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>Query string parameters for the get home assessments endpoint.</summary>
public sealed class GetHomeAssessmentsQuerySpec : PaginationQuerySpecV2
{
    /// <summary>Initializes a new instance with optional pagination.</summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    public GetHomeAssessmentsQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize)
        : base(pageNumber, pageSize)
    {
    }
}
