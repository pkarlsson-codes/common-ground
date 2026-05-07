using CommonGround.SharedKernel.Domain;

namespace CommonGround.Stewardship.Application.Commands.StewardshipCases;

/// <summary>
/// Command to escalate a violation for a stewardship case.
/// </summary>
/// <param name="StewardshipCaseId">The ID of the stewardship case.</param>
/// <param name="ViolationId">The ID of the violation to escalate.</param>
/// <param name="Reason">The reason for escalation.</param>
/// <param name="EscalatedDate">The date the violation was escalated.</param>
/// <param name="EscalatedBy">The user ID of who escalated the violation.</param>
public sealed record EscalateViolationCommand(
    string StewardshipCaseId,
    Guid ViolationId,
    string Reason,
    DateOnly EscalatedDate,
    UserId EscalatedBy);
