using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Query string parameters for the get purchase workflows endpoint.</summary>
public sealed class GetPurchaseWorkflowsQuerySpec : PaginationQuerySpecV2
{
    /// <summary>Initializes a new instance with optional filters.</summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="groundLeaseTemplateId">Optional ground lease template ID filter.</param>
    /// <param name="phase">Optional workflow phase filter.</param>
    /// <param name="search">Optional free-text search across buyer name.</param>
    public GetPurchaseWorkflowsQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        string? groundLeaseTemplateId = null,
        string? phase = null,
        string? search = null)
        : base(pageNumber, pageSize)
    {
        GroundLeaseTemplateId = groundLeaseTemplateId;
        Phase = phase;
        Search = search;
    }

    /// <summary>Gets an optional ground lease template ID filter.</summary>
    public string? GroundLeaseTemplateId { get; }

    /// <summary>Gets an optional workflow phase filter.</summary>
    public string? Phase { get; }

    /// <summary>Gets optional free-text search.</summary>
    public string? Search { get; }

    /// <inheritdoc cref="PaginationQuerySpecV2.AppendFilterParts"/>
    protected override void AppendFilterParts(List<string> parts)
    {
        AddIfNotWhiteSpace(parts, "groundLeaseTemplateId", GroundLeaseTemplateId);
        AddIfNotWhiteSpace(parts, "phase", Phase);
        AddIfNotWhiteSpace(parts, "search", Search);
    }
}
