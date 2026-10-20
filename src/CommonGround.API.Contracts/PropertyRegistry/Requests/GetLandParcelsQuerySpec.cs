using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>Query string parameters for the get land parcels endpoint.</summary>
public sealed class GetLandParcelsQuerySpec : PaginationQuerySpecV2
{
    /// <summary>Initializes a new instance with optional filters.</summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="search">Optional free-text search across address and legal parcel number.</param>
    /// <param name="includeDecommissioned">Whether to include decommissioned parcels.</param>
    public GetLandParcelsQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        bool includeDecommissioned = false)
        : base(pageNumber, pageSize)
    {
        Search = search;
        IncludeDecommissioned = includeDecommissioned;
    }

    /// <summary>Gets optional free-text search.</summary>
    public string? Search { get; }

    /// <summary>Gets whether decommissioned parcels are included.</summary>
    public bool IncludeDecommissioned { get; }

    /// <inheritdoc cref="PaginationQuerySpecV2.AppendFilterParts"/>
    protected override void AppendFilterParts(List<string> parts)
    {
        AddIfNotWhiteSpace(parts, "search", Search);

        if (IncludeDecommissioned)
        {
            parts.Add("includeDecommissioned=true");
        }
    }
}
