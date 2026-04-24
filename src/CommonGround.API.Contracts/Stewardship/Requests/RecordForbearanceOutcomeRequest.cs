namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Request to record the outcome of a forbearance request.</summary>
public sealed record RecordForbearanceOutcomeRequest(
    string Outcome,
    DateOnly OutcomeDate,
    string? Notes);
