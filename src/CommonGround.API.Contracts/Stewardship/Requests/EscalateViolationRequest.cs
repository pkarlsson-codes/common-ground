namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Request to escalate an unresolved lease violation.</summary>
public sealed record EscalateViolationRequest(
    string Reason,
    DateOnly EscalatedDate);
