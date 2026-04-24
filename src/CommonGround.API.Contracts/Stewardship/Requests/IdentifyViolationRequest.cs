namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Request to identify a new lease violation on a stewardship case.</summary>
public sealed record IdentifyViolationRequest(
    string ViolationType,
    string Description,
    DateOnly IdentifiedDate);
