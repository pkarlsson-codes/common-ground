namespace CommonGround.API.Contracts.ApplicantSelection.Responses;

/// <summary>Summary item for a homeowner application candidate for a home.</summary>
public sealed record HomeCandidateResponse(
    string Id,
    string HouseholdId,
    HouseholdContactDto? PrimaryContact);
