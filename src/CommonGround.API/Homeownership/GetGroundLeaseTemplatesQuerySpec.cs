using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Homeownership;

/// <summary>Query string parameters for the get ground lease templates endpoint.</summary>
public sealed class GetGroundLeaseTemplatesQuerySpec : PaginationQuerySpecV2
{
    /// <summary>Initializes a new instance with optional filters.</summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="search">Optional name filter.</param>
    /// <param name="status">Optional status filter.</param>
    public GetGroundLeaseTemplatesQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? status = null)
        : base(pageNumber, pageSize)
    {
        Search = search;
        Status = status;
    }

    /// <summary>Gets the optional name search term.</summary>
    public string? Search { get; }

    /// <summary>Gets the optional status filter.</summary>
    public string? Status { get; }
}
