namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>A lease and what it owes in lease fees as of the trust's today.</summary>
/// <param name="GroundLeaseId">The lease.</param>
/// <param name="HomeId">The leased home.</param>
/// <param name="HomeLabel">The home's label, if known.</param>
/// <param name="HomeownerFullName">The homeowner's name.</param>
/// <param name="LeaseStatus">Active or Inactive.</param>
/// <param name="MonthlyFee">The fee charged on the 1st of each month.</param>
/// <param name="Balance">Above zero a balance due, below zero a credit.</param>
/// <param name="OldestUnpaidDueDate">Due date of the oldest charge not fully paid, if any.</param>
/// <param name="DaysOverdue">Days since that charge fell due; zero when nothing is owed.</param>
/// <param name="NoticeDue">The late notice due now, First or Second, or null.</param>
/// <param name="LastNoticeLevel">The level of the last notice sent since the arrears began, or null.</param>
/// <param name="LastNoticeSentOn">The day that notice was sent, or null.</param>
public sealed record LeaseFeeBalanceResponse(
    string GroundLeaseId,
    string HomeId,
    string? HomeLabel,
    string? HomeownerFullName,
    string LeaseStatus,
    decimal MonthlyFee,
    decimal Balance,
    DateOnly? OldestUnpaidDueDate,
    int DaysOverdue,
    string? NoticeDue,
    string? LastNoticeLevel,
    DateOnly? LastNoticeSentOn);

/// <summary>The late notices to print, one letter per lease.</summary>
/// <param name="Today">The trust's today, the date on every letter.</param>
/// <param name="LetterSignOff">The trust's sign-off for the end of every letter; empty for none.</param>
/// <param name="Notices">Each lease with a notice due, most days overdue first.</param>
public sealed record LeaseFeeNoticesDueResponse(
    DateOnly Today,
    string LetterSignOff,
    IReadOnlyList<LeaseFeeBalanceResponse> Notices);

/// <summary>A trust's lease fee notice policy.</summary>
/// <param name="FirstNoticeDays">Days overdue before the first notice.</param>
/// <param name="SecondNoticeDays">Days overdue before the second notice.</param>
/// <param name="LetterSignOff">The lines printed at the end of every letter; empty for none.</param>
/// <param name="IsDefault">Whether the trust never set a policy, so these are the defaults.</param>
public sealed record LeaseFeeNoticePolicyResponse(
    int FirstNoticeDays,
    int SecondNoticeDays,
    string LetterSignOff,
    bool IsDefault);

/// <summary>The result of recording a late notice.</summary>
/// <param name="NoticeId">The new notice's ID.</param>
public sealed record RecordLeaseFeeNoticeResponse(string NoticeId);
