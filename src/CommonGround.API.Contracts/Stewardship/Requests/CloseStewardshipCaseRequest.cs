namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Request to close a stewardship case.</summary>
public sealed record CloseStewardshipCaseRequest(
    string Reason,
    DateOnly ClosedDate);
