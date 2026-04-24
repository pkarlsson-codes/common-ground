namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Request to record staff approval of an occupancy verification submission.</summary>
public sealed record VerifyOccupancyRequest(
    DateOnly VerifiedDate);
