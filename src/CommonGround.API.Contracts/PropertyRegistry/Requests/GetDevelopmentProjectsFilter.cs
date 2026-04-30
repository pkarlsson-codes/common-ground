namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>
/// Filter parameters for retrieving development projects.
/// </summary>
public sealed class GetDevelopmentProjectsFilter
{
    /// <summary>Optional free-text search across description.</summary>
    public string? Search { get; init; }
}
