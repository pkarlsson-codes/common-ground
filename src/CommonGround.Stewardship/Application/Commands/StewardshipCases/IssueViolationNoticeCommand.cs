namespace CommonGround.Stewardship.Application.Commands.StewardshipCases;

/// <summary>
/// Command to issue a violation notice for a stewardship case.
/// </summary>
/// <param name="StewardshipCaseId">The ID of the stewardship case.</param>
/// <param name="ViolationId">The ID of the violation.</param>
/// <param name="NoticeDate">The date the notice was issued.</param>
/// <param name="CureDeadline">The deadline to cure the violation.</param>
/// <param name="IssuedBy">The user ID of who issued the notice.</param>
public sealed record IssueViolationNoticeCommand(
    string StewardshipCaseId,
    Guid ViolationId,
    DateOnly NoticeDate,
    DateOnly CureDeadline,
    string IssuedBy);
