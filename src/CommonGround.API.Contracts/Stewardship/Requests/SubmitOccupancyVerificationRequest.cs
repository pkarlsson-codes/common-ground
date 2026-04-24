namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Request to submit homeowner responses to an occupancy verification.</summary>
public sealed record SubmitOccupancyVerificationRequest(
    bool IsPrimaryResident,
    bool SubletDisclosed,
    string? MajorChanges,
    DateOnly SubmittedDate);
