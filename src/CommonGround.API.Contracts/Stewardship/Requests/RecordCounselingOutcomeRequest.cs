namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Request to record the outcome of a counseling referral.</summary>
public sealed record RecordCounselingOutcomeRequest(
    string Outcome,
    DateOnly OutcomeDate);
