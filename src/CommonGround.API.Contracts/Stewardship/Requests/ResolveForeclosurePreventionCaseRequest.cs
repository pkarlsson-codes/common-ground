namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Request to resolve a foreclosure prevention case with a final outcome.</summary>
public sealed record ResolveForeclosurePreventionCaseRequest(
    string Outcome,
    DateOnly ResolvedDate);
