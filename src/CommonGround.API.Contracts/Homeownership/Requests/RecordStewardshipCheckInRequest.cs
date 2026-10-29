namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record a stewardship check-in visit.</summary>
public sealed record RecordStewardshipCheckInRequest(
    DateOnly CheckInDate,
    string Notes);
