using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>Query string parameters for the get households endpoint.</summary>
public sealed class GetHouseholdsQuerySpec : PaginationQuerySpecV2
{
    /// <summary>Initializes a new instance with optional filters.</summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="search">Optional text matched against any member's name or email.</param>
    /// <param name="email">Optional email that must equal a member's email, ignoring case and surrounding whitespace.</param>
    public GetHouseholdsQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        string? email = null)
        : base(pageNumber, pageSize)
    {
        Search = search;
        Email = email;
    }

    /// <summary>Gets optional text matched against any member's name or email.</summary>
    public string? Search { get; }

    /// <summary>Gets an optional email that must equal a member's email, ignoring case and surrounding whitespace.</summary>
    public string? Email { get; }

    /// <inheritdoc cref="PaginationQuerySpecV2.AppendFilterParts"/>
    protected override void AppendFilterParts(List<string> parts)
    {
        AddIfNotWhiteSpace(parts, "search", Search);
        AddIfNotWhiteSpace(parts, "email", Email);
    }
}
