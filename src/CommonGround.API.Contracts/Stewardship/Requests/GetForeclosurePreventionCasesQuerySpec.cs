using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Query parameters for listing foreclosure prevention cases with optional ground lease filter and pagination.</summary>
public sealed class GetForeclosurePreventionCasesQuerySpec : PaginationQuerySpecV2
{
    /// <summary>Initializes a new instance of <see cref="GetForeclosurePreventionCasesQuerySpec"/>.</summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="groundLeaseId">Ground lease ID filter, or null to return all non-resolved cases.</param>
    public GetForeclosurePreventionCasesQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        string? groundLeaseId = null)
        : base(pageNumber, pageSize)
    {
        GroundLeaseId = groundLeaseId;
    }

    /// <summary>Gets the ground lease ID to filter by, or null to return all non-resolved cases.</summary>
    public string? GroundLeaseId { get; }

    /// <inheritdoc cref="PaginationQuerySpecV2.AppendFilterParts"/>
    protected override void AppendFilterParts(List<string> parts)
    {
        AddIfNotWhiteSpace(parts, "groundLeaseId", GroundLeaseId);
    }
}
