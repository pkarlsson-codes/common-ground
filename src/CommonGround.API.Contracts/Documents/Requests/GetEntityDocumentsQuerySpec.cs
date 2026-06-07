using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Contracts.Documents.Requests;

/// <summary>Query parameters for listing entity document attachments with pagination.</summary>
public sealed class GetEntityDocumentsQuerySpec : PaginationQuerySpecV2
{
    /// <summary>Initializes a new instance with optional filters.</summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="includeDeleted">Whether to include soft-deleted attachments.</param>
    public GetEntityDocumentsQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        bool includeDeleted = false)
        : base(pageNumber, pageSize)
    {
        IncludeDeleted = includeDeleted;
    }

    /// <summary>Gets whether soft-deleted attachments are included.</summary>
    public bool IncludeDeleted { get; }

    /// <inheritdoc cref="PaginationQuerySpecV2.AppendFilterParts"/>
    protected override void AppendFilterParts(List<string> parts)
    {
        if (IncludeDeleted)
        {
            parts.Add("includeDeleted=true");
        }
    }
}
