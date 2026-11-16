namespace CommonGround.Homeownership.Application.ReadModels;

public class GroundLeaseView
{
    public string Id { get; set; } = string.Empty;

    public string? GroundLeaseTemplateId { get; set; } = string.Empty;

    /// <summary>Gets or sets the purchase workflow ID if this lease was created from a home purchase; otherwise null/empty.</summary>
    public string? PurchaseWorkflowId { get; set; } = string.Empty;

    public string LandParcelId { get; set; } = string.Empty;

    public string HomeId { get; set; } = string.Empty;

    public DateOnly LeaseStartDate { get; set; }

    public int TermYears { get; set; }

    /// <summary>Gets or sets the days the trust has to buy the home after its homeowner gives notice to sell.</summary>
    public int OptionWindowDays { get; set; }

    /// <summary>Gets or sets what the homeowner paid for the home, which the resale formula starts from.</summary>
    public decimal BasePrice { get; set; }

    /// <summary>Gets or sets the home's appraised value when the homeowner bought it, if known.</summary>
    public decimal? OriginalAppraisedValue { get; set; }

    public string CovenantTerms { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public string FormulaKind { get; set; } = string.Empty;

    /// <summary>Gets or sets the appreciation share (for AppraisalBased formula).</summary>
    public decimal? AppreciationShare { get; set; }

    /// <summary>Gets or sets the index type (for IndexBased formula).</summary>
    public string? IndexType { get; set; }

    /// <summary>Gets or sets the baseline index value (for IndexBased formula).</summary>
    public decimal? BaselineIndexValue { get; set; }

    /// <summary>Gets or sets the annual percentage (for FixedPercentage formula).</summary>
    public decimal? AnnualPercentage { get; set; }

    public decimal? ImprovementsCap { get; set; }

    public string? HomeownerFullName { get; set; }

    public string? HomeownerEmail { get; set; }

    public string? HomeownerPhone { get; set; }

    /// <summary>Gets or sets who held the home before the current homeowner, in the order the home passed on.</summary>
    public List<PreviousHomeownerEntry> PreviousHomeowners { get; set; } = [];

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

    /// <summary>
    /// Gets or sets the insurance the lease has required: what it started with, then each change in effective-date
    /// order.
    /// </summary>
    public List<InsuranceRequirementsEntry> InsuranceRequirements { get; set; } = [];

    /// <summary>Gets or sets every insurance certificate recorded, removed ones included, in the order recorded.</summary>
    public List<InsuranceCertificateEntry> InsuranceCertificates { get; set; } = [];
}

public class InsuranceCertificateEntry
{
    public string CertificateId { get; set; } = string.Empty;

    public string Insurer { get; set; } = string.Empty;

    public string PolicyNumber { get; set; } = string.Empty;

    public DateOnly PolicyStart { get; set; }

    public DateOnly PolicyEnd { get; set; }

    public decimal DwellingCoverage { get; set; }

    public decimal LiabilityPerOccurrence { get; set; }

    public bool TrustNamedAdditionalInsured { get; set; }

    public DateOnly ReceivedOn { get; set; }

    /// <summary>Gets or sets the notes, or null for none.</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets the certificate file in Documents, or null when none was added.</summary>
    public string? DocumentId { get; set; }

    public string RecordedBy { get; set; } = string.Empty;

    public DateTimeOffset RecordedAt { get; set; }

    /// <summary>Gets or sets why it was removed, or null while it counts.</summary>
    public string? RemovedReason { get; set; }

    public string? RemovedBy { get; set; }

    public DateTimeOffset? RemovedAt { get; set; }
}

/// <summary>The insurance a lease requires from a day on, in a ground lease view.</summary>
public class InsuranceRequirementsEntry
{
    /// <summary>Gets or sets the first day the requirements apply; the lease start for the ones it started with.</summary>
    public DateOnly EffectiveFrom { get; set; }

    /// <summary>Gets or sets the replacement value the dwelling coverage must reach, or null when not checked.</summary>
    public decimal? ReplacementValue { get; set; }

    /// <summary>Gets or sets the liability coverage per occurrence required, or null when not checked.</summary>
    public decimal? LiabilityPerOccurrence { get; set; }

    public string SetBy { get; set; } = string.Empty;

    public DateTimeOffset SetAt { get; set; }
}

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

    public decimal Price { get; set; }

    public string RecordedBy { get; set; } = string.Empty;

    public DateTimeOffset RecordedAt { get; set; }
}

public class PermittedMortgageEntry
{
    public string MortgageId { get; set; } = string.Empty;

    public string Lender { get; set; } = string.Empty;

    /// <summary>Gets or sets where the trust sends copies of notices, or null when not on file.</summary>
    public string? NoticeAddress { get; set; }

    public decimal OriginalPrincipal { get; set; }

    public DateOnly PermittedOn { get; set; }

    /// <summary>Gets or sets how the trust permitted it, as the source's display name.</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>Gets or sets the day it was paid off, or null while it is current.</summary>
    public DateOnly? PaidOffOn { get; set; }
}

public class LeaseFeeInterestWaiverEntry
{
    public string WaiverId { get; set; } = string.Empty;

    public DateOnly WaivedOn { get; set; }

    public decimal Amount { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string WaivedBy { get; set; } = string.Empty;
}

public class LeaseFeeNoticeEntry
{
    public string NoticeId { get; set; } = string.Empty;

    /// <summary>Gets or sets which notice it was, as the level's display name.</summary>
    public string Level { get; set; } = string.Empty;

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

    public string RecordedBy { get; set; } = string.Empty;

    public DateTimeOffset RecordedAt { get; set; }
}

public class MortgageeCopyEntry
{
    public string MortgageId { get; set; } = string.Empty;

    public string Lender { get; set; } = string.Empty;

    /// <summary>Gets or sets where the copy went; null when none was on file.</summary>
    public string? NoticeAddress { get; set; }
}

public class LeaseFeePaymentEntry
{
    public string PaymentId { get; set; } = string.Empty;

    public DateOnly ReceivedDate { get; set; }

    public decimal Amount { get; set; }

    /// <summary>Gets or sets how it was paid, as the payment method's display name.</summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>Gets or sets a check number or similar, if given.</summary>
    public string? Reference { get; set; }

    public string RecordedBy { get; set; } = string.Empty;

    public DateTimeOffset RecordedAt { get; set; }

    /// <summary>Gets or sets why it was voided, or null while it counts.</summary>
    public string? VoidReason { get; set; }

    public string? VoidedBy { get; set; }

    public DateTimeOffset? VoidedAt { get; set; }
}

public class StewardshipCheckInEntry
{
    public DateOnly CheckInDate { get; set; }

    public string Notes { get; set; } = string.Empty;

    public string RecordedBy { get; set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; set; }
}

public class PreviousHomeownerEntry
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    /// <summary>Gets or sets the day the home passed to the next homeowner.</summary>
    public DateOnly TransferredOn { get; set; }

    public string HeirTransferId { get; set; } = string.Empty;
}
