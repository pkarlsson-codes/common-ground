using CommonGround.SharedKernel.Infrastructure.Querying;

namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>Query string parameters for the get homes endpoint.</summary>
public sealed class GetHomesQuerySpec : PaginationQuerySpecV2
{
    /// <summary>Initializes a new instance with optional filters.</summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="landParcelIds">Optional land parcel ID filters.</param>
    /// <param name="statuses">Optional status filters.</param>
    public GetHomesQuerySpec(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        IReadOnlyList<string>? landParcelIds = null,
        IReadOnlyList<string>? statuses = null)
        : base(pageNumber, pageSize)
    {
        LandParcelIds = landParcelIds;
        Statuses = statuses;
    }

    /// <summary>Gets optional land parcel ID filters.</summary>
    public IReadOnlyList<string>? LandParcelIds { get; }

    /// <summary>Gets optional status filters.</summary>
    public IReadOnlyList<string>? Statuses { get; }

    /// <inheritdoc cref="PaginationQuerySpecV2.AppendFilterParts"/>
    protected override void AppendFilterParts(List<string> parts)
    {
        AddEach(parts, "landParcelIds", LandParcelIds);
        AddEach(parts, "statuses", Statuses);
    }
}
