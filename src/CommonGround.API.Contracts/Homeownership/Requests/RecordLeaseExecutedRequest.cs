namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record lease executed.</summary>
public sealed record RecordLeaseExecutedRequest(DateOnly SignedDate);
