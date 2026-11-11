using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Query string parameters for the lease fee balances endpoint.</summary>
public sealed class GetLeaseFeeBalancesQuerySpec : PaginationQuerySpecV2
{
    /// <summary>Initializes a new instance with optional filters.</summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="includeAll">List leases with nothing due too, not only those with a balance due.</param>
    /// <param name="search">Matches the homeowner's name or the home's label.</param>
    /// <param name="noticeDueOnly">List only leases with a late notice due; wins over <paramref name="includeAll"/>.</param>
    public GetLeaseFeeBalancesQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        bool includeAll = false,
        string? search = null,
        bool noticeDueOnly = false)
        : base(pageNumber, pageSize)
    {
        IncludeAll = includeAll;
        Search = search;
        NoticeDueOnly = noticeDueOnly;
    }

    /// <summary>Gets whether only leases with a late notice due are listed.</summary>
    public bool NoticeDueOnly { get; }

    /// <summary>Gets whether leases with nothing due are listed too.</summary>
    public bool IncludeAll { get; }

    /// <summary>Gets an optional search term.</summary>
    public string? Search { get; }

    /// <inheritdoc cref="PaginationQuerySpecV2.AppendFilterParts"/>
    protected override void AppendFilterParts(List<string> parts)
    {
        if (IncludeAll)
            parts.Add("includeAll=true");
        AddIfNotWhiteSpace(parts, "search", Search);
        if (NoticeDueOnly)
            parts.Add("noticeDueOnly=true");
    }
}
