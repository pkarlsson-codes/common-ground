using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>Query string parameters for the get home selections endpoint.</summary>
public sealed class GetHomeSelectionsQuerySpec : PaginationQuerySpecV2
{
    /// <summary>Initializes a new instance with optional filters.</summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="status">Optional selection status filter.</param>
    public GetHomeSelectionsQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        string? status = null)
        : base(pageNumber, pageSize)
    {
        Status = status;
    }

    /// <summary>Gets an optional selection status filter.</summary>
    public string? Status { get; }

    /// <inheritdoc cref="PaginationQuerySpecV2.AppendFilterParts"/>
    protected override void AppendFilterParts(List<string> parts)
    {
        AddIfNotWhiteSpace(parts, "status", Status);
    }
}
