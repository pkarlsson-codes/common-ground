namespace CommonGround.Stewardship.Application.Commands.ForeclosurePreventionCases;

/// <summary>
/// Command to record the outcome of counseling for a foreclosure prevention case.
/// </summary>
/// <param name="ForeclosurePreventionCaseId">The ID of the foreclosure prevention case.</param>
/// <param name="ReferralId">The ID of the counseling referral.</param>
/// <param name="Outcome">The outcome of the counseling.</param>
/// <param name="OutcomeDate">The date the outcome occurred.</param>
/// <param name="RecordedBy">The user ID of who recorded the outcome.</param>
public sealed record RecordCounselingOutcomeCommand(
    string ForeclosurePreventionCaseId,
    Guid ReferralId,
    string Outcome,
    DateOnly OutcomeDate,
    string RecordedBy);
