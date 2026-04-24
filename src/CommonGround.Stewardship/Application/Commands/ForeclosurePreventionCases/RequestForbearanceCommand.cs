namespace CommonGround.Stewardship.Application.Commands.ForeclosurePreventionCases;

/// <summary>
/// Command to request forbearance for a foreclosure prevention case.
/// </summary>
/// <param name="ForeclosurePreventionCaseId">The ID of the foreclosure prevention case.</param>
/// <param name="LenderName">The name of the lender.</param>
/// <param name="RequestedDate">The date forbearance was requested.</param>
/// <param name="RequestedBy">The user ID of who requested forbearance.</param>
public sealed record RequestForbearanceCommand(
    string ForeclosurePreventionCaseId,
    string LenderName,
    DateOnly RequestedDate,
    string RequestedBy);
