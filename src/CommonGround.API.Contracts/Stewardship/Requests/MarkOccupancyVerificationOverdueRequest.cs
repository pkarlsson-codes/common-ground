namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Request to mark an occupancy verification as overdue.</summary>
public sealed record MarkOccupancyVerificationOverdueRequest(
    DateOnly Deadline);
