namespace CommonGround.Homeownership.Application.ReadModels;

/// <summary>Detail read model for a ground lease.</summary>
public class GroundLeaseView
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the ground lease template ID used when creating this lease.</summary>
    public string? GroundLeaseTemplateId { get; set; } = string.Empty;

    /// <summary>Gets or sets the purchase workflow ID if this lease was created from a home purchase; otherwise null/empty.</summary>
    public string? PurchaseWorkflowId { get; set; } = string.Empty;

    /// <summary>Gets or sets the land parcel ID.</summary>
    public string LandParcelId { get; set; } = string.Empty;

    /// <summary>Gets or sets the home ID.</summary>
    public string HomeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the lease start date.</summary>
    public DateOnly LeaseStartDate { get; set; }

    /// <summary>Gets or sets the lease term in years.</summary>
    public int TermYears { get; set; }

    /// <summary>Gets or sets the days the trust has to buy the home after its homeowner gives notice to sell.</summary>
    public int OptionWindowDays { get; set; }

    /// <summary>Gets or sets what the homeowner paid for the home, which the resale formula starts from.</summary>
    public decimal BasePrice { get; set; }

    /// <summary>Gets or sets the home's appraised value when the homeowner bought it, if known.</summary>
    public decimal? OriginalAppraisedValue { get; set; }

    /// <summary>Gets or sets the covenant terms.</summary>
    public string CovenantTerms { get; set; } = string.Empty;

    /// <summary>Gets or sets the lease status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the user who created this lease.</summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets when the lease was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the resale formula kind.</summary>
    public string FormulaKind { get; set; } = string.Empty;

    /// <summary>Gets or sets the appreciation share (for AppraisalBased formula).</summary>
    public decimal? AppreciationShare { get; set; }

    /// <summary>Gets or sets the index type (for IndexBased formula).</summary>
    public string? IndexType { get; set; }

    /// <summary>Gets or sets the baseline index value (for IndexBased formula).</summary>
    public decimal? BaselineIndexValue { get; set; }

    /// <summary>Gets or sets the annual percentage (for FixedPercentage formula).</summary>
    public decimal? AnnualPercentage { get; set; }

    /// <summary>Gets or sets the improvements cap.</summary>
    public decimal? ImprovementsCap { get; set; }

    /// <summary>Gets or sets the current homeowner's full name.</summary>
    public string? HomeownerFullName { get; set; }

    /// <summary>Gets or sets the current homeowner's email.</summary>
    public string? HomeownerEmail { get; set; }

    /// <summary>Gets or sets the current homeowner's phone number.</summary>
    public string? HomeownerPhone { get; set; }

    /// <summary>Gets or sets the stewardship check-in history.</summary>
    public List<StewardshipCheckInEntry> StewardshipCheckIns { get; set; } = [];

    /// <summary>Gets or sets the monthly land use fee.</summary>
    public decimal LandUseFee { get; set; }

    /// <summary>Gets or sets the monthly repair reserve fee.</summary>
    public decimal RepairReserveFee { get; set; }

    /// <summary>Gets or sets the yearly interest rate on a late fee, in percent; zero charges none.</summary>
    public decimal LateInterestRate { get; set; }

    /// <summary>Gets or sets the days after the due date within which a payment owes no interest.</summary>
    public int InterestWaiverDays { get; set; }

    /// <summary>Gets or sets the debt cap, a whole percent of the purchase option price.</summary>
    public int MaxMortgageDebtPercent { get; set; }

    /// <summary>Gets or sets the first day the trust tracks the lease's fees in CommonGround.</summary>
    public DateOnly FeesTrackedFrom { get; set; }

    /// <summary>Gets or sets what the homeowner owed on <see cref="FeesTrackedFrom"/>.</summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>Gets or sets the day the lease ended, or null while it is active.</summary>
    public DateOnly? EndedOn { get; set; }

    /// <summary>Gets or sets every lease fee payment recorded, voided ones included, in the order recorded.</summary>
    public List<LeaseFeePaymentEntry> LeaseFeePayments { get; set; } = [];

    /// <summary>Gets or sets every late notice recorded, in the order recorded.</summary>
    public List<LeaseFeeNoticeEntry> LeaseFeeNotices { get; set; } = [];

    /// <summary>Gets or sets every late-fee interest waiver, in the order recorded.</summary>
    public List<LeaseFeeInterestWaiverEntry> LeaseFeeInterestWaivers { get; set; } = [];

    /// <summary>Gets or sets every permitted mortgage, paid-off ones included, in the order recorded.</summary>
    public List<PermittedMortgageEntry> PermittedMortgages { get; set; } = [];

    /// <summary>Gets or sets the resale in progress on the lease, or null when none is.</summary>
    public string? OpenResaleId { get; set; }

    /// <summary>Gets or sets every resale price statement sent, in the order recorded.</summary>
    public List<ResalePriceStatementEntry> ResalePriceStatements { get; set; } = [];
}

/// <summary>A resale price statement sent to the homeowner, in a ground lease view.</summary>
public class ResalePriceStatementEntry
{
    /// <summary>Gets or sets the letter's date, the day the price was worked out on and sent.</summary>
    public DateOnly StatementDate { get; set; }

    /// <summary>Gets or sets how it was sent, as the delivery's display name.</summary>
    public string Delivery { get; set; } = string.Empty;

    /// <summary>Gets or sets the full years from the lease start to the statement date.</summary>
    public int YearsOwned { get; set; }

    /// <summary>Gets or sets the estimated market value used; null unless the formula is appraisal based.</summary>
    public decimal? MarketValue { get; set; }

    /// <summary>Gets or sets where the market value came from.</summary>
    public string? MarketValueSource { get; set; }

    /// <summary>Gets or sets the index value used; null unless the formula is index based.</summary>
    public decimal? IndexValue { get; set; }

    /// <summary>Gets or sets the improvements credit counted, after the cap; null for none.</summary>
    public decimal? ImprovementsCredit { get; set; }

    /// <summary>Gets or sets the price stated.</summary>
    public decimal Price { get; set; }

    /// <summary>Gets or sets the user who recorded it.</summary>
    public string RecordedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets when it was recorded.</summary>
    public DateTimeOffset RecordedAt { get; set; }
}

/// <summary>A mortgage the trust permitted, in a ground lease view.</summary>
public class PermittedMortgageEntry
{
    /// <summary>Gets or sets the mortgage ID.</summary>
    public string MortgageId { get; set; } = string.Empty;

    /// <summary>Gets or sets the lender or servicer.</summary>
    public string Lender { get; set; } = string.Empty;

    /// <summary>Gets or sets where the trust sends copies of notices, or null when not on file.</summary>
    public string? NoticeAddress { get; set; }

    /// <summary>Gets or sets the loan amount when it closed.</summary>
    public decimal OriginalPrincipal { get; set; }

    /// <summary>Gets or sets the day the trust permitted it.</summary>
    public DateOnly PermittedOn { get; set; }

    /// <summary>Gets or sets how the trust permitted it, as the source's display name.</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>Gets or sets the day it was paid off, or null while it is current.</summary>
    public DateOnly? PaidOffOn { get; set; }
}

/// <summary>Late-fee interest waived, in a ground lease view.</summary>
public class LeaseFeeInterestWaiverEntry
{
    /// <summary>Gets or sets the waiver ID.</summary>
    public string WaiverId { get; set; } = string.Empty;

    /// <summary>Gets or sets the day it was waived.</summary>
    public DateOnly WaivedOn { get; set; }

    /// <summary>Gets or sets how much interest was waived.</summary>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets why.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Gets or sets the user who waived it.</summary>
    public string WaivedBy { get; set; } = string.Empty;
}

/// <summary>A late lease fee notice in a ground lease view.</summary>
public class LeaseFeeNoticeEntry
{
    /// <summary>Gets or sets the notice ID.</summary>
    public string NoticeId { get; set; } = string.Empty;

    /// <summary>Gets or sets which notice it was, as the level's display name.</summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>Gets or sets the day it was sent.</summary>
    public DateOnly SentOn { get; set; }

    /// <summary>Gets or sets how it was sent, as the delivery's display name.</summary>
    public string Delivery { get; set; } = string.Empty;

    /// <summary>Gets or sets what the lease owed that day.</summary>
    public decimal BalanceDue { get; set; }

    /// <summary>Gets or sets the due date of the oldest unpaid charge that day.</summary>
    public DateOnly OldestUnpaidDueDate { get; set; }

    /// <summary>Gets or sets a notice of default's cure days; null for other notices.</summary>
    public int? CureDays { get; set; }

    /// <summary>Gets or sets a notice of default's extension days; null for other notices.</summary>
    public int? CureExtensionDays { get; set; }

    /// <summary>Gets or sets the permitted mortgagees a notice of default was copied to; null for other notices.</summary>
    public List<MortgageeCopyEntry>? CopiedTo { get; set; }

    /// <summary>Gets or sets the user who recorded it.</summary>
    public string RecordedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets when it was recorded.</summary>
    public DateTimeOffset RecordedAt { get; set; }
}

/// <summary>A permitted mortgagee sent a copy of a notice of default, in a ground lease view.</summary>
public class MortgageeCopyEntry
{
    /// <summary>Gets or sets the permitted mortgage ID.</summary>
    public string MortgageId { get; set; } = string.Empty;

    /// <summary>Gets or sets the lender or servicer.</summary>
    public string Lender { get; set; } = string.Empty;

    /// <summary>Gets or sets where the copy went; null when none was on file.</summary>
    public string? NoticeAddress { get; set; }
}

/// <summary>A lease fee payment in a ground lease view.</summary>
public class LeaseFeePaymentEntry
{
    /// <summary>Gets or sets the payment ID.</summary>
    public string PaymentId { get; set; } = string.Empty;

    /// <summary>Gets or sets the day the trust received it.</summary>
    public DateOnly ReceivedDate { get; set; }

    /// <summary>Gets or sets the amount received.</summary>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets how it was paid, as the payment method's display name.</summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>Gets or sets a check number or similar, if given.</summary>
    public string? Reference { get; set; }

    /// <summary>Gets or sets the user who recorded it.</summary>
    public string RecordedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets when it was recorded.</summary>
    public DateTimeOffset RecordedAt { get; set; }

    /// <summary>Gets or sets why it was voided, or null while it counts.</summary>
    public string? VoidReason { get; set; }

    /// <summary>Gets or sets the user who voided it.</summary>
    public string? VoidedBy { get; set; }

    /// <summary>Gets or sets when it was voided.</summary>
    public DateTimeOffset? VoidedAt { get; set; }
}

/// <summary>A single stewardship check-in entry in a ground lease view.</summary>
public class StewardshipCheckInEntry
{
    /// <summary>Gets or sets the check-in date.</summary>
    public DateOnly CheckInDate { get; set; }

    /// <summary>Gets or sets the check-in notes.</summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>Gets or sets the user who recorded the check-in.</summary>
    public string RecordedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets when the check-in was recorded.</summary>
    public DateTimeOffset OccurredAt { get; set; }
}
