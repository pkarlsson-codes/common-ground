namespace CommonGround.SharedKernel.Infrastructure.Querying;

/// <summary>
/// Constructor-based pagination query specification for flat query-string binding.
/// </summary>
public abstract class PaginationQuerySpecV2
{
    /// <summary>
    /// Default page size when none is specified.
    /// </summary>
    public const int DefaultPageSize = 20;

    /// <summary>
    /// Maximum allowed page size.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Initializes a new pagination query specification from flat paging arguments.
    /// </summary>
    /// <param name="pageNumber">1-based page number from query-string.</param>
    /// <param name="pageSize">Page size from query-string.</param>
    protected PaginationQuerySpecV2(int pageNumber = 1, int pageSize = DefaultPageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        Page = new PageSpec
        {
            Number = NormalizePageNumber(pageNumber),
            Size = NormalizePageSize(pageSize),
        };
    }

    /// <summary>
    /// Gets the incoming 1-based page number prior to normalization.
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Gets the incoming page size prior to normalization.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Gets normalized pagination values for downstream query handling.
    /// </summary>
    public PageSpec Page { get; }

    /// <summary>
    /// Builds the query string for this specification.
    /// </summary>
    /// <returns>A query string beginning with '?' containing all pagination and filter parameters.</returns>
    public string ToQueryString()
    {
        var parts = new List<string>
        {
            $"pageNumber={PageNumber}",
            $"pageSize={PageSize}",
        };

        AppendFilterParts(parts);

        return "?" + string.Join("&", parts);
    }

    /// <summary>
    /// Appends a key=value pair if the value is not null or whitespace.
    /// </summary>
    /// <param name="parts">The list of key=value parts to append to.</param>
    /// <param name="key">The query parameter key.</param>
    /// <param name="value">The value to conditionally append.</param>
    protected static void AddIfNotWhiteSpace(List<string> parts, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parts.Add($"{key}={Uri.EscapeDataString(value)}");
        }
    }

    /// <summary>
    /// Appends a key=value pair for each non-whitespace element in the collection.
    /// </summary>
    /// <param name="parts">The list of key=value parts to append to.</param>
    /// <param name="key">The query parameter key.</param>
    /// <param name="values">The values to conditionally append.</param>
    protected static void AddEach(List<string> parts, string key, IEnumerable<string>? values)
    {
        if (values is null)
        {
            return;
        }

        foreach (var v in values)
        {
            if (!string.IsNullOrWhiteSpace(v))
            {
                parts.Add($"{key}={Uri.EscapeDataString(v)}");
            }
        }
    }

    /// <summary>
    /// Override to append filter-specific query string parts between the page parameters.
    /// </summary>
    /// <param name="parts">The list of key=value parts to append to.</param>
    protected virtual void AppendFilterParts(List<string> parts)
    {
    }

    private static int NormalizePageNumber(int pageNumber)
    {
        return pageNumber < 1 ? 1 : pageNumber;
    }

    private static int NormalizePageSize(int pageSize)
    {
        if (pageSize < 1)
        {
            return DefaultPageSize;
        }

        return pageSize > MaxPageSize ? MaxPageSize : pageSize;
    }
}
