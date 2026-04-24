namespace CommonGround.Stewardship.Application.Commands.StewardshipCases;

/// <summary>
/// Command to initiate occupancy verification for a stewardship case.
/// </summary>
/// <param name="StewardshipCaseId">The ID of the stewardship case.</param>
/// <param name="PeriodYear">The year for the verification period.</param>
/// <param name="SurveyDeadline">Optional deadline for the survey submission.</param>
/// <param name="InitiatedBy">The user ID of who initiated the verification.</param>
public sealed record InitiateOccupancyVerificationCommand(
    string StewardshipCaseId,
    int PeriodYear,
    DateOnly? SurveyDeadline,
    string InitiatedBy);
