namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>Detail response for a ground lease.</summary>
public sealed record GroundLeaseResponse(
    string Id,
    string? GroundLeaseTemplateId,
    string? PurchaseWorkflowId,
    string LandParcelId,
    string HomeId,
    DateOnly LeaseStartDate,
    int TermYears,
    int OptionWindowDays,
    decimal BasePrice,
    decimal? OriginalAppraisedValue,
    string CovenantTerms,
    string Status,
    string FormulaKind,
    decimal? AppreciationShare,
    string? IndexType,
    decimal? BaselineIndexValue,
    decimal? AnnualPercentage,
    decimal? ImprovementsCap,
    string? HomeownerFullName,
    string? HomeownerEmail,
    string? HomeownerPhone,
    IReadOnlyList<StewardshipCheckInDto> StewardshipCheckIns,
    LeaseFeesDto LeaseFees,
    DateTimeOffset CreatedAt,
    DateOnly RecordedOn,
    string CreatedBy);

/// <summary>A lease's fee terms, what it owes as of the trust's today, and its payments.</summary>
/// <param name="LandUseFee">Monthly land use fee.</param>
/// <param name="RepairReserveFee">Monthly repair reserve fee.</param>
/// <param name="MonthlyFee">Their sum, charged on the 1st of each month.</param>
/// <param name="LateInterestRate">Yearly interest rate on a late fee, in percent; zero charges none.</param>
/// <param name="InterestWaiverDays">Days after the due date within which a payment owes no interest.</param>
/// <param name="Statement">What the lease owes today.</param>
/// <param name="Payments">Every payment recorded, voided ones included, newest received first.</param>
/// <param name="NoticeDue">The late notice due now, First or Second, or null.</param>
/// <param name="Notices">Every late notice recorded, newest sent first.</param>
/// <param name="InterestWaivers">Every interest waiver, newest first.</param>
public sealed record LeaseFeesDto(
    decimal LandUseFee,
    decimal RepairReserveFee,
    decimal MonthlyFee,
    decimal LateInterestRate,
    int InterestWaiverDays,
    LeaseFeeStatementDto Statement,
    IReadOnlyList<LeaseFeePaymentDto> Payments,
    string? NoticeDue,
    IReadOnlyList<LeaseFeeNoticeDto> Notices,
    IReadOnlyList<LeaseFeeInterestWaiverDto> InterestWaivers);

/// <summary>Late-fee interest the trust waived.</summary>
/// <param name="WaiverId">The waiver's ID.</param>
/// <param name="WaivedOn">The day it was waived.</param>
/// <param name="Amount">How much was waived.</param>
/// <param name="Reason">Why.</param>
/// <param name="WaivedBy">The user who waived it.</param>
public sealed record LeaseFeeInterestWaiverDto(
    string WaiverId,
    DateOnly WaivedOn,
    decimal Amount,
    string Reason,
    string WaivedBy);

/// <summary>A late lease fee notice the trust sent.</summary>
/// <param name="NoticeId">The notice's ID.</param>
/// <param name="Level">Which notice: First or Second.</param>
/// <param name="SentOn">The day it was sent.</param>
/// <param name="Delivery">How it was sent: FirstClassMail, CertifiedMail, RegisteredMail, HandDelivered or Email.</param>
/// <param name="BalanceDue">What the lease owed that day.</param>
/// <param name="OldestUnpaidDueDate">The due date of the oldest unpaid charge that day.</param>
/// <param name="RecordedBy">The user who recorded it.</param>
/// <param name="RecordedAt">When it was recorded.</param>
public sealed record LeaseFeeNoticeDto(
    string NoticeId,
    string Level,
    DateOnly SentOn,
    string Delivery,
    decimal BalanceDue,
    DateOnly OldestUnpaidDueDate,
    string RecordedBy,
    DateTimeOffset RecordedAt);

/// <summary>What a lease owes as of a day.</summary>
/// <param name="AsOf">The day it is worked out for.</param>
/// <param name="TotalCharged">Total charged so far.</param>
/// <param name="TotalPaid">Total of the payments that count.</param>
/// <param name="Balance">Fees plus interest due, less payments: above zero a balance due, below zero a credit.</param>
/// <param name="OldestUnpaidDueDate">Due date of the oldest charge not fully paid, if any.</param>
/// <param name="DaysOverdue">Days since that charge fell due; zero when nothing is owed.</param>
/// <param name="ArrearsSince">Due date of the first charge left unpaid since the lease was last paid up, if any.</param>
/// <param name="InterestCharged">Interest on fees paid or unpaid past the waiver days, before waivers.</param>
/// <param name="InterestWaived">Interest the trust waived.</param>
/// <param name="InterestDue">Interest owed after waivers; part of the balance.</param>
/// <param name="InterestAccruing">Interest running on fees still within the waiver days; not in the balance.</param>
/// <param name="InterestWaivedIfPaidBy">The last day the oldest unpaid fee can be paid without interest, if that is still ahead.</param>
public sealed record LeaseFeeStatementDto(
    DateOnly AsOf,
    decimal TotalCharged,
    decimal TotalPaid,
    decimal Balance,
    DateOnly? OldestUnpaidDueDate,
    int DaysOverdue,
    DateOnly? ArrearsSince,
    decimal InterestCharged,
    decimal InterestWaived,
    decimal InterestDue,
    decimal InterestAccruing,
    DateOnly? InterestWaivedIfPaidBy);

/// <summary>A lease fee payment.</summary>
/// <param name="PaymentId">The payment's ID.</param>
/// <param name="ReceivedDate">The day the trust received it.</param>
/// <param name="Amount">The amount received.</param>
/// <param name="Method">How it was paid: Check, BankTransfer, Cash, MoneyOrder, Card or Other.</param>
/// <param name="Reference">A check number or similar, if given.</param>
/// <param name="RecordedBy">The user who recorded it.</param>
/// <param name="RecordedAt">When it was recorded.</param>
/// <param name="VoidReason">Why it was voided, or null while it counts.</param>
/// <param name="VoidedAt">When it was voided, if it was.</param>
public sealed record LeaseFeePaymentDto(
    string PaymentId,
    DateOnly ReceivedDate,
    decimal Amount,
    string Method,
    string? Reference,
    string RecordedBy,
    DateTimeOffset RecordedAt,
    string? VoidReason,
    DateTimeOffset? VoidedAt);

/// <summary>DTO for a stewardship check-in entry.</summary>
public sealed record StewardshipCheckInDto(
    DateOnly CheckInDate,
    string Notes,
    string RecordedBy,
    DateTimeOffset OccurredAt);
