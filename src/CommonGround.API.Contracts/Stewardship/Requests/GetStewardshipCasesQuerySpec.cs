using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Query parameters for listing stewardship cases with optional ground lease filter and pagination.</summary>
public sealed class GetStewardshipCasesQuerySpec : PaginationQuerySpecV2
{
    /// <summary>Initializes a new instance of <see cref="GetStewardshipCasesQuerySpec"/>.</summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="groundLeaseId">Ground lease ID filter, or null to return all active cases.</param>
    public GetStewardshipCasesQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        string? groundLeaseId = null)
        : base(pageNumber, pageSize)
    {
        GroundLeaseId = groundLeaseId;
    }

    /// <summary>Gets the ground lease ID to filter by, or null to return all active cases.</summary>
    public string? GroundLeaseId { get; }

    /// <inheritdoc cref="PaginationQuerySpecV2.AppendFilterParts"/>
    protected override void AppendFilterParts(List<string> parts)
    {
        AddIfNotWhiteSpace(parts, "groundLeaseId", GroundLeaseId);
    }
}
