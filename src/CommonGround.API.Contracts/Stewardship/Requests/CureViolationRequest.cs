namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Request to record that a lease violation has been cured.</summary>
public sealed record CureViolationRequest(
    DateOnly CuredDate);
