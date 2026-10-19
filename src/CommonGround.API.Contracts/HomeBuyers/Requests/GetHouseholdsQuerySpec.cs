using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>Query string parameters for the get households endpoint.</summary>
public sealed class GetHouseholdsQuerySpec : PaginationQuerySpecV2
{
    /// <summary>Initializes a new instance with optional filters.</summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="search">Optional text matched against any member's name or email.</param>
    public GetHouseholdsQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        string? search = null)
        : base(pageNumber, pageSize)
    {
        Search = search;
    }

    /// <summary>Gets optional text matched against any member's name or email.</summary>
    public string? Search { get; }

    /// <inheritdoc cref="PaginationQuerySpecV2.AppendFilterParts"/>
    protected override void AppendFilterParts(List<string> parts)
    {
        AddIfNotWhiteSpace(parts, "search", Search);
    }
}
