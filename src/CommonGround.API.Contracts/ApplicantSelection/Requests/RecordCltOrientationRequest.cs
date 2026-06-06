namespace CommonGround.API.Contracts.ApplicantSelection.Requests;

/// <summary>Request to record CLT orientation completion.</summary>
public sealed record RecordCltOrientationRequest(string CompletedDate);