namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Filter parameters for retrieving purchase workflows.</summary>
public sealed class GetPurchaseWorkflowsFilter
{
    /// <summary>Optional ground lease ID filter.</summary>
    public string? GroundLeaseId { get; init; }

    /// <summary>Optional phase filter.</summary>
    public string? Phase { get; init; }

    /// <summary>Optional free-text search across buyer full name.</summary>
    public string? Search { get; init; }
}
