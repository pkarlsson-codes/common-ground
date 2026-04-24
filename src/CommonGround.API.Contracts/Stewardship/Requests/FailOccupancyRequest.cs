namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Request to fail an occupancy verification.</summary>
public sealed record FailOccupancyRequest(
    string Reason,
    DateOnly FailedDate);
