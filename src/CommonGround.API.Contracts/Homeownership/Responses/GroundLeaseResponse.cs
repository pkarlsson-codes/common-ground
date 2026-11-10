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
/// <param name="Statement">What the lease owes today.</param>
/// <param name="Payments">Every payment recorded, voided ones included, newest received first.</param>
public sealed record LeaseFeesDto(
    decimal LandUseFee,
    decimal RepairReserveFee,
    decimal MonthlyFee,
    LeaseFeeStatementDto Statement,
    IReadOnlyList<LeaseFeePaymentDto> Payments);

/// <summary>What a lease owes as of a day.</summary>
/// <param name="AsOf">The day it is worked out for.</param>
/// <param name="TotalCharged">Total charged so far.</param>
/// <param name="TotalPaid">Total of the payments that count.</param>
/// <param name="Balance">Above zero a balance due, below zero a credit.</param>
/// <param name="OldestUnpaidDueDate">Due date of the oldest charge not fully paid, if any.</param>
/// <param name="DaysOverdue">Days since that charge fell due; zero when nothing is owed.</param>
public sealed record LeaseFeeStatementDto(
    DateOnly AsOf,
    decimal TotalCharged,
    decimal TotalPaid,
    decimal Balance,
    DateOnly? OldestUnpaidDueDate,
    int DaysOverdue);

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
