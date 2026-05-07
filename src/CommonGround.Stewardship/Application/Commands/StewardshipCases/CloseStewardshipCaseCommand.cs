using CommonGround.SharedKernel.Domain;

namespace CommonGround.Stewardship.Application.Commands.StewardshipCases;

/// <summary>
/// Command to close a stewardship case.
/// </summary>
/// <param name="StewardshipCaseId">The ID of the stewardship case.</param>
/// <param name="Reason">The reason for closing the case.</param>
/// <param name="ClosedDate">The date the case was closed.</param>
/// <param name="ClosedBy">The user ID of who closed the case.</param>
public sealed record CloseStewardshipCaseCommand(
    string StewardshipCaseId,
    string Reason,
    DateOnly ClosedDate,
    UserId ClosedBy);
