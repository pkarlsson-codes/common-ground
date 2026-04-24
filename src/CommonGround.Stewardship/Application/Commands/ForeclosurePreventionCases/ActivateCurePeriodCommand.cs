namespace CommonGround.Stewardship.Application.Commands.ForeclosurePreventionCases;

/// <summary>
/// Command to activate a cure period for a foreclosure prevention case.
/// </summary>
/// <param name="ForeclosurePreventionCaseId">The ID of the foreclosure prevention case.</param>
/// <param name="LraId">The ID of the LRA (Local Recipient Agency).</param>
/// <param name="ActivationDate">The date the cure period is activated.</param>
/// <param name="CurePeriodDays">The number of days in the cure period.</param>
/// <param name="ActivatedBy">The user ID of who activated the cure period.</param>
public sealed record ActivateCurePeriodCommand(
    string ForeclosurePreventionCaseId,
    string LraId,
    DateOnly ActivationDate,
    int CurePeriodDays,
    string ActivatedBy);
