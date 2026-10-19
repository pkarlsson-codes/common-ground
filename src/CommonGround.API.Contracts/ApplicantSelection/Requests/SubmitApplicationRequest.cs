namespace CommonGround.API.Contracts.ApplicantSelection.Requests;

/// <summary>Request to submit a new homeowner application.</summary>
public sealed record SubmitApplicationRequest(
    string HouseholdId);