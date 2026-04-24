namespace CommonGround.Stewardship.Application.Commands.StewardshipCases;

/// <summary>
/// Command to cure a violation for a stewardship case.
/// </summary>
/// <param name="StewardshipCaseId">The ID of the stewardship case.</param>
/// <param name="ViolationId">The ID of the violation to cure.</param>
/// <param name="CuredDate">The date the violation was cured.</param>
/// <param name="VerifiedBy">The user ID of who verified the cure.</param>
public sealed record CureViolationCommand(
    string StewardshipCaseId,
    Guid ViolationId,
    DateOnly CuredDate,
    string VerifiedBy);
