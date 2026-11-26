using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>Query string parameters for the get home selections endpoint.</summary>
public sealed class GetHomeSelectionsQuerySpec : PaginationQuerySpecV2
{
    /// <summary>Initializes a new instance.</summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    public GetHomeSelectionsQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize)
        : base(pageNumber, pageSize)
    {
    }
}
