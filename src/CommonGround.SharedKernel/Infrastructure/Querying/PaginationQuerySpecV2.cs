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
