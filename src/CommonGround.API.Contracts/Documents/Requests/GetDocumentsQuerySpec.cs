using System.Globalization;
using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Contracts.Documents.Requests;

/// <summary>Query parameters for listing documents with optional filters and pagination.</summary>
public sealed class GetDocumentsQuerySpec : PaginationQuerySpecV2
{
    /// <summary>Initializes a new instance with optional filters.</summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="includeDeleted">Whether to include soft-deleted documents.</param>
    /// <param name="contentType">Content-type filter, or null to return all.</param>
    /// <param name="tag">Tag filter, or null to return all.</param>
    /// <param name="expiringBefore">Upper bound on expiry date, or null for no filter.</param>
    /// <param name="search">Optional case-insensitive text matched against the title.</param>
    public GetDocumentsQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        bool includeDeleted = false,
        string? contentType = null,
        string? tag = null,
        DateOnly? expiringBefore = null,
        string? search = null)
        : base(pageNumber, pageSize)
    {
        IncludeDeleted = includeDeleted;
        ContentType = contentType;
        Tag = tag;
        ExpiringBefore = expiringBefore;
        Search = search;
    }

    /// <summary>Gets whether soft-deleted documents are included.</summary>
    public bool IncludeDeleted { get; }

    /// <summary>Gets the content-type filter, or null to return all.</summary>
    public string? ContentType { get; }

    /// <summary>Gets the tag filter, or null to return all.</summary>
    public string? Tag { get; }

    /// <summary>Gets the upper bound on expiry date, or null for no filter.</summary>
    public DateOnly? ExpiringBefore { get; }

    /// <summary>Gets optional case-insensitive text matched against the title.</summary>
    public string? Search { get; }

    /// <inheritdoc cref="PaginationQuerySpecV2.AppendFilterParts"/>
    protected override void AppendFilterParts(List<string> parts)
    {
        if (IncludeDeleted)
        {
            parts.Add("includeDeleted=true");
        }

        AddIfNotWhiteSpace(parts, "contentType", ContentType);
        AddIfNotWhiteSpace(parts, "tag", Tag);
        AddIfNotWhiteSpace(parts, "search", Search);

        if (ExpiringBefore.HasValue)
        {
            parts.Add($"expiringBefore={Uri.EscapeDataString(ExpiringBefore.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}");
        }
    }
}
