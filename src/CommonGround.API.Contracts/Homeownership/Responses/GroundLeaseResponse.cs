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
    decimal ApprovedImprovements,
    string? HomeownerFullName,
    string? HomeownerEmail,
    string? HomeownerPhone,
    IReadOnlyList<StewardshipCheckInDto> StewardshipCheckIns,
    LeaseFeesDto LeaseFees,
    MortgagesDto Mortgages,
    ResalePriceStatementsDto ResalePriceStatements,
    InsuranceDto Insurance,
    DateTimeOffset CreatedAt,
    DateOnly RecordedOn,
    string CreatedBy);

/// <summary>The insurance a lease requires (Art 9.4) and the certificates the homeowner sent.</summary>
/// <param name="RequiredReplacementValue">The replacement value required as of the trust's today, or null when not checked.</param>
/// <param name="RequiredLiabilityPerOccurrence">The liability per occurrence required as of the trust's today, or null when not checked.</param>
/// <param name="RequirementsHistory">What the lease started with and each change since, latest effective first.</param>
/// <param name="CoveredThrough">The latest policy end among the certificates not removed, or null when none is on file.</param>
/// <param name="CertificateDueOn">When the next certificate is due: the covered-through date, or the lease start with none on file.</param>
/// <param name="CurrentCheck">How the current certificate compares with the lease, or null when none is on file.</param>
/// <param name="Certificates">Every certificate recorded, removed ones included, latest policy end first.</param>
public sealed record InsuranceDto(
    decimal? RequiredReplacementValue,
    decimal? RequiredLiabilityPerOccurrence,
    IReadOnlyList<InsuranceRequirementsChangeDto> RequirementsHistory,
    DateOnly? CoveredThrough,
    DateOnly CertificateDueOn,
    InsuranceCheckDto? CurrentCheck,
    IReadOnlyList<InsuranceCertificateDto> Certificates);

/// <summary>How the current certificate compares with the requirements in effect on its policy start.</summary>
/// <param name="MeetsLease">Whether it meets every requirement that is set.</param>
/// <param name="Shortfalls">Each way it falls short: DwellingBelowReplacementValue, LiabilityBelowRequired, TrustNotAdditionalInsured.</param>
/// <param name="ReplacementValue">The replacement value it was checked against, or null when not checked.</param>
/// <param name="LiabilityPerOccurrence">The liability per occurrence it was checked against, or null when not checked.</param>
public sealed record InsuranceCheckDto(
    bool MeetsLease,
    IReadOnlyList<string> Shortfalls,
    decimal? ReplacementValue,
    decimal? LiabilityPerOccurrence);

/// <summary>A certificate of the homeowner's insurance.</summary>
/// <param name="CertificateId">The certificate ID.</param>
/// <param name="Insurer">The insurer.</param>
/// <param name="PolicyNumber">The policy number.</param>
/// <param name="PolicyStart">The first day of the policy period.</param>
/// <param name="PolicyEnd">The last day of the policy period.</param>
/// <param name="DwellingCoverage">The dwelling coverage.</param>
/// <param name="LiabilityPerOccurrence">The liability coverage per occurrence.</param>
/// <param name="TrustNamedAdditionalInsured">Whether the trust is named as additional insured.</param>
/// <param name="ReceivedOn">The day the trust received it.</param>
/// <param name="Notes">Notes, or null for none.</param>
/// <param name="DocumentId">The certificate file in Documents, or null when none was added.</param>
/// <param name="RecordedBy">The user who recorded it.</param>
/// <param name="RecordedAt">When it was recorded.</param>
/// <param name="RemovedReason">Why it was removed, or null while it counts.</param>
/// <param name="RemovedAt">When it was removed, or null while it counts.</param>
public sealed record InsuranceCertificateDto(
    string CertificateId,
    string Insurer,
    string PolicyNumber,
    DateOnly PolicyStart,
    DateOnly PolicyEnd,
    decimal DwellingCoverage,
    decimal LiabilityPerOccurrence,
    bool TrustNamedAdditionalInsured,
    DateOnly ReceivedOn,
    string? Notes,
    string? DocumentId,
    string RecordedBy,
    DateTimeOffset RecordedAt,
    string? RemovedReason,
    DateTimeOffset? RemovedAt);

/// <summary>The insurance a lease requires from a day on.</summary>
/// <param name="EffectiveFrom">The first day they apply; the lease start for the ones it started with.</param>
/// <param name="ReplacementValue">The replacement value required, or null when not checked.</param>
/// <param name="LiabilityPerOccurrence">The liability per occurrence required, or null when not checked.</param>
/// <param name="SetBy">The user who set them.</param>
/// <param name="SetAt">When they were set.</param>
public sealed record InsuranceRequirementsChangeDto(
    DateOnly EffectiveFrom,
    decimal? ReplacementValue,
    decimal? LiabilityPerOccurrence,
    string SetBy,
    DateTimeOffset SetAt);

/// <summary>A lease's yearly resale price statements (Ch 23, Disclosure).</summary>
/// <param name="NextDueOn">When the next statement is due; null for an ended lease or one with a resale in progress.</param>
/// <param name="ResaleInProgress">Whether a resale is in progress, so no statement is due.</param>
/// <param name="Statements">Every statement sent, newest first.</param>
public sealed record ResalePriceStatementsDto(
    DateOnly? NextDueOn,
    bool ResaleInProgress,
    IReadOnlyList<ResalePriceStatementDto> Statements);

/// <summary>A resale price statement sent to the homeowner, with the figures used.</summary>
/// <param name="StatementDate">The letter's date, the day the price was worked out on and sent.</param>
/// <param name="Delivery">How it was sent.</param>
/// <param name="YearsOwned">Full years from the lease start to the statement date.</param>
/// <param name="MarketValue">The estimated market value used; null unless the formula is appraisal based.</param>
/// <param name="MarketValueSource">Where the market value came from.</param>
/// <param name="IndexValue">The index value used; null unless the formula is index based.</param>
/// <param name="ImprovementsCredit">The improvements credit counted, after the cap; null for none.</param>
/// <param name="Price">The most the home could sell for on the statement date.</param>
/// <param name="RecordedBy">The user who recorded it.</param>
/// <param name="RecordedAt">When it was recorded.</param>
public sealed record ResalePriceStatementDto(
    DateOnly StatementDate,
    string Delivery,
    int YearsOwned,
    decimal? MarketValue,
    string? MarketValueSource,
    decimal? IndexValue,
    decimal? ImprovementsCredit,
    decimal Price,
    string RecordedBy,
    DateTimeOffset RecordedAt);

/// <summary>A lease's debt cap and the mortgages the trust permitted on it.</summary>
/// <param name="MaxMortgageDebtPercent">The debt cap (Art 8.3), a whole percent of the purchase option price.</param>
/// <param name="Permitted">Every permitted mortgage, current ones first, then paid off; each newest permitted first.</param>
public sealed record MortgagesDto(
    int MaxMortgageDebtPercent,
    IReadOnlyList<PermittedMortgageDto> Permitted);

/// <summary>A mortgage the trust permitted on a lease.</summary>
/// <param name="MortgageId">The mortgage's ID.</param>
/// <param name="Lender">The lender or servicer.</param>
/// <param name="NoticeAddress">Where the trust sends copies of notices, or null when not on file.</param>
/// <param name="OriginalPrincipal">The loan amount when it closed.</param>
/// <param name="PermittedOn">The day the trust permitted it.</param>
/// <param name="Source">How: LeaseSigning, Request or Recorded.</param>
/// <param name="PaidOffOn">The day it was paid off, or null while it is current.</param>
public sealed record PermittedMortgageDto(
    string MortgageId,
    string Lender,
    string? NoticeAddress,
    decimal OriginalPrincipal,
    DateOnly PermittedOn,
    string Source,
    DateOnly? PaidOffOn);

/// <summary>A lease's fee terms, what it owes as of the trust's today, and its payments.</summary>
/// <param name="LandUseFee">Monthly land use fee.</param>
/// <param name="RepairReserveFee">Monthly repair reserve fee.</param>
/// <param name="MonthlyFee">Their sum, charged on the 1st of each month.</param>
/// <param name="LateInterestRate">Yearly interest rate on a late fee, in percent; zero charges none.</param>
/// <param name="InterestWaiverDays">Days after the due date within which a payment owes no interest.</param>
/// <param name="FeesTrackedFrom">The first day the trust tracks the lease's fees in CommonGround.</param>
/// <param name="OpeningBalance">What the homeowner owed on that day.</param>
/// <param name="Statement">What the lease owes today.</param>
/// <param name="Payments">Every payment recorded, voided ones included, newest received first.</param>
/// <param name="NoticeDue">The late notice due now, First, Second or Default, or null.</param>
/// <param name="Notices">Every late notice recorded, newest sent first.</param>
/// <param name="InterestWaivers">Every interest waiver, newest first.</param>
/// <param name="DefaultCure">Where the last notice of default stands, or null if none was sent.</param>
public sealed record LeaseFeesDto(
    decimal LandUseFee,
    decimal RepairReserveFee,
    decimal MonthlyFee,
    decimal LateInterestRate,
    int InterestWaiverDays,
    DateOnly FeesTrackedFrom,
    decimal OpeningBalance,
    LeaseFeeStatementDto Statement,
    IReadOnlyList<LeaseFeePaymentDto> Payments,
    string? NoticeDue,
    IReadOnlyList<LeaseFeeNoticeDto> Notices,
    IReadOnlyList<LeaseFeeInterestWaiverDto> InterestWaivers,
    DefaultCureDto? DefaultCure);

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
/// <param name="Level">Which notice: First, Second or Default.</param>
/// <param name="SentOn">The day it was sent.</param>
/// <param name="Delivery">How it was sent: FirstClassMail, CertifiedMail, RegisteredMail, HandDelivered or Email.</param>
/// <param name="BalanceDue">What the lease owed that day.</param>
/// <param name="OldestUnpaidDueDate">The due date of the oldest unpaid charge that day.</param>
/// <param name="RecordedBy">The user who recorded it.</param>
/// <param name="RecordedAt">When it was recorded.</param>
/// <param name="CureDays">A notice of default's cure days; null for other notices.</param>
/// <param name="CureExtensionDays">A notice of default's extension days; null for other notices.</param>
/// <param name="CopiedTo">The permitted mortgagees a notice of default was copied to; null for other notices.</param>
public sealed record LeaseFeeNoticeDto(
    string NoticeId,
    string Level,
    DateOnly SentOn,
    string Delivery,
    decimal BalanceDue,
    DateOnly OldestUnpaidDueDate,
    string RecordedBy,
    DateTimeOffset RecordedAt,
    int? CureDays,
    int? CureExtensionDays,
    IReadOnlyList<MortgageeCopyDto>? CopiedTo);

/// <summary>A permitted mortgagee who gets, or got, a copy of a notice of default.</summary>
/// <param name="Lender">The lender or servicer.</param>
/// <param name="NoticeAddress">Where the copy goes; null when none is on file.</param>
public sealed record MortgageeCopyDto(string Lender, string? NoticeAddress);

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
