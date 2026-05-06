using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Documents;

/// <summary>
/// Endpoint-specific query spec for listing entity document attachments.
/// Keeps HTTP binding concerns separate from shared pagination specs.
/// </summary>
public sealed class GetEntityDocumentsQuerySpec : PaginationQuerySpecV2
{
    /// <summary>
    /// Initializes a new endpoint query spec from flat query-string values.
    /// </summary>
    /// <param name="pageNumber">The 1-based page number from the <c>pageNumber</c> query key.</param>
    /// <param name="pageSize">The page size from the <c>pageSize</c> query key.</param>
    /// <param name="includeDeleted">Whether deleted attachments are included from the <c>includeDeleted</c> query key.</param>
    public GetEntityDocumentsQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        bool includeDeleted = false)
        : base(pageNumber, pageSize)
    {
        IncludeDeleted = includeDeleted;
    }

    /// <summary>
    /// Gets whether deleted attachments are included from the <c>includeDeleted</c> query key.
    /// </summary>
    public bool IncludeDeleted { get; }
}