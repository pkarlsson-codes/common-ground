namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Request to cancel a scheduled property inspection.</summary>
public sealed record CancelPropertyInspectionRequest(
    string Reason);
