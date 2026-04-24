namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Request to issue a formal notice for a lease violation.</summary>
public sealed record IssueViolationNoticeRequest(
    DateOnly NoticeDate,
    DateOnly CureDeadline);
