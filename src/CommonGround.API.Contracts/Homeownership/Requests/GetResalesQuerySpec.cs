using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Query string parameters for the get resales endpoint.</summary>
public sealed class GetResalesQuerySpec : PaginationQuerySpecV2
{
    /// <summary>Initializes a new instance with optional filters.</summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="status">Optional status filter: Open or Withdrawn.</param>
    /// <param name="groundLeaseId">Optional ground lease ID filter.</param>
    public GetResalesQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        string? status = null,
        string? groundLeaseId = null)
        : base(pageNumber, pageSize)
    {
        Status = status;
        GroundLeaseId = groundLeaseId;
    }

    /// <summary>Gets an optional status filter.</summary>
    public string? Status { get; }

    /// <summary>Gets an optional ground lease ID filter.</summary>
    public string? GroundLeaseId { get; }

    /// <inheritdoc cref="PaginationQuerySpecV2.AppendFilterParts"/>
    protected override void AppendFilterParts(List<string> parts)
    {
        AddIfNotWhiteSpace(parts, "status", Status);
        AddIfNotWhiteSpace(parts, "groundLeaseId", GroundLeaseId);
    }
}
