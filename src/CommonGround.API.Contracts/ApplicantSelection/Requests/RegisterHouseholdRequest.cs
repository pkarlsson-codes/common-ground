namespace CommonGround.API.Contracts.ApplicantSelection.Requests;

/// <summary>Request to register a new household.</summary>
public sealed record RegisterHouseholdRequest(
    IReadOnlyList<HouseholdMemberRequest> Members,
    bool RequiresAccessibility,
    decimal AnnualIncome);

/// <summary>A household member supplied when registering a household.</summary>
public sealed record HouseholdMemberRequest(
    string Name,
    string Email,
    string Phone,
    bool IsPrimary);
