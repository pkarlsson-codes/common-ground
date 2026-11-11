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
/// <param name="NoticeDue">The late notice due now, First, Second or Default, or null.</param>
/// <param name="LastNoticeLevel">The level of the last notice sent since the arrears began, or null.</param>
/// <param name="LastNoticeSentOn">The day that notice was sent, or null.</param>
/// <param name="LateInterestRate">The lease's yearly interest rate on a late fee, in percent; zero charges none.</param>
/// <param name="InterestWaiverDays">Days after the due date within which a payment owes no interest.</param>
/// <param name="InterestDue">Interest owed after waivers; part of the balance.</param>
/// <param name="InterestAccruing">Interest running on fees still within the waiver days; not in the balance.</param>
/// <param name="InterestWaivedIfPaidBy">The last day the oldest unpaid fee can be paid without interest, if that is still ahead.</param>
/// <param name="DefaultCure">Where the lease's last notice of default stands, or null if none was sent.</param>
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
    DateOnly? LastNoticeSentOn,
    decimal LateInterestRate,
    int InterestWaiverDays,
    decimal InterestDue,
    decimal InterestAccruing,
    DateOnly? InterestWaivedIfPaidBy,
    DefaultCureDto? DefaultCure);

/// <summary>Where a notice of default stands as of the trust's today (Art 12.1).</summary>
/// <param name="SentOn">The day the notice was sent.</param>
/// <param name="AmountOwed">What the notice said was owed.</param>
/// <param name="OriginalDeadline">The cure deadline before any extension.</param>
/// <param name="CureBy">The last day to pay, extended when two-thirds was paid by the original deadline.</param>
/// <param name="Extended">Whether the deadline was extended.</param>
/// <param name="PaidSinceNotice">The total received since the notice was sent.</param>
/// <param name="StillToPay">What is still to pay of the amount owed; zero once paid.</param>
/// <param name="PaidInFullOn">The day payments since the notice reached the amount owed, if they have.</param>
/// <param name="Status">Curing, Cured, PaidAfterDeadline or Uncured.</param>
public sealed record DefaultCureDto(
    DateOnly SentOn,
    decimal AmountOwed,
    DateOnly OriginalDeadline,
    DateOnly CureBy,
    bool Extended,
    decimal PaidSinceNotice,
    decimal StillToPay,
    DateOnly? PaidInFullOn,
    string Status);

/// <summary>The late notices to print, one letter per lease.</summary>
/// <param name="Today">The trust's today, the date on every letter.</param>
/// <param name="LetterSignOff">The trust's sign-off for the end of every letter; empty for none.</param>
/// <param name="CureDays">Days a notice of default gives to pay, for its letter.</param>
/// <param name="CureExtensionDays">Days added when two-thirds is paid within the cure days, for its letter.</param>
/// <param name="Notices">Each lease with a notice due, most days overdue first.</param>
public sealed record LeaseFeeNoticesDueResponse(
    DateOnly Today,
    string LetterSignOff,
    int CureDays,
    int CureExtensionDays,
    IReadOnlyList<LeaseFeeBalanceResponse> Notices);

/// <summary>A trust's lease fee notice policy.</summary>
/// <param name="FirstNoticeDays">Days overdue before the first notice.</param>
/// <param name="SecondNoticeDays">Days overdue before the second notice.</param>
/// <param name="DefaultNoticeDays">Days overdue before the notice of default.</param>
/// <param name="CureDays">Days a notice of default gives to pay.</param>
/// <param name="CureExtensionDays">Days added when two-thirds is paid within the cure days.</param>
/// <param name="LetterSignOff">The lines printed at the end of every letter; empty for none.</param>
/// <param name="IsDefault">Whether the trust never set a policy, so these are the defaults.</param>
public sealed record LeaseFeeNoticePolicyResponse(
    int FirstNoticeDays,
    int SecondNoticeDays,
    int DefaultNoticeDays,
    int CureDays,
    int CureExtensionDays,
    string LetterSignOff,
    bool IsDefault);

/// <summary>The result of recording a late notice.</summary>
/// <param name="NoticeId">The new notice's ID.</param>
public sealed record RecordLeaseFeeNoticeResponse(string NoticeId);

/// <summary>Response after waiving late-fee interest.</summary>
/// <param name="WaiverId">The new waiver's ID.</param>
public sealed record WaiveLeaseFeeInterestResponse(string WaiverId);
