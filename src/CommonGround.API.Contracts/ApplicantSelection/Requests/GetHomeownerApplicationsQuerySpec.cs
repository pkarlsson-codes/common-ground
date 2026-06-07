using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Contracts.ApplicantSelection.Requests;

/// <summary>Query string parameters for the get homeowner applications endpoint.</summary>
public sealed class GetHomeownerApplicationsQuerySpec : PaginationQuerySpecV2
{
    /// <summary>Initializes a new instance with optional filters.</summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="phase">Optional application phase filter.</param>
    /// <param name="search">Optional free-text search across applicant fields.</param>
    public GetHomeownerApplicationsQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        string? phase = null,
        string? search = null)
        : base(pageNumber, pageSize)
    {
        Phase = phase;
        Search = search;
    }

    /// <summary>Gets an optional application phase filter.</summary>
    public string? Phase { get; }

    /// <summary>Gets optional free-text search.</summary>
    public string? Search { get; }

    /// <inheritdoc cref="PaginationQuerySpecV2.AppendFilterParts"/>
    protected override void AppendFilterParts(List<string> parts)
    {
        AddIfNotWhiteSpace(parts, "phase", Phase);
        AddIfNotWhiteSpace(parts, "search", Search);
    }
}
