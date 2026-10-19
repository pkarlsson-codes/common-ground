using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Contracts.HomeBuyers.Requests;

/// <summary>Query string parameters for the get homeowner applications endpoint.</summary>
public sealed class GetHomeownerApplicationsQuerySpec : PaginationQuerySpecV2
{
    /// <summary>Initializes a new instance with optional filters.</summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="phase">Optional application phase filter.</param>
    public GetHomeownerApplicationsQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        string? phase = null)
        : base(pageNumber, pageSize)
    {
        Phase = phase;
    }

    /// <summary>Gets an optional application phase filter.</summary>
    public string? Phase { get; }

    /// <inheritdoc cref="PaginationQuerySpecV2.AppendFilterParts"/>
    protected override void AppendFilterParts(List<string> parts)
    {
        AddIfNotWhiteSpace(parts, "phase", Phase);
    }
}
