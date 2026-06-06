namespace CommonGround.API.Contracts.ApplicantSelection.Requests;

/// <summary>Filter parameters for retrieving homeowner applications.</summary>
public sealed class GetHomeownerApplicationsFilter
{
    /// <summary>Optional phase filter.</summary>
    public string? Phase { get; init; }

    /// <summary>Optional free-text search across applicant fields.</summary>
    public string? Search { get; init; }
}