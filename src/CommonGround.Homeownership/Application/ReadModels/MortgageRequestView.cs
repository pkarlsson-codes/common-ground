namespace CommonGround.Homeownership.Application.ReadModels;

/// <summary>Read model for a homeowner's request to put a new loan on the home.</summary>
public class MortgageRequestView
{
    /// <summary>Gets or sets the request ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the homeowner's lease.</summary>
    public string GroundLeaseId { get; set; } = string.Empty;

    /// <summary>Gets or sets the home the loan is secured on.</summary>
    public string HomeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the homeowner's full name when the request arrived.</summary>
    public string? HomeownerFullName { get; set; }

    /// <summary>Gets or sets Pending, Approved, Denied, Closed or Withdrawn.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the day the request reached the trust.</summary>
    public DateOnly ReceivedOn { get; set; }

    /// <summary>Gets or sets the lender making the loan.</summary>
    public string Lender { get; set; } = string.Empty;

    /// <summary>Gets or sets what the loan is for.</summary>
    public string Purpose { get; set; } = string.Empty;

    /// <summary>Gets or sets the loan amount.</summary>
    public decimal Principal { get; set; }

    /// <summary>Gets or sets all mortgage debt on the home once the loan closes.</summary>
    public decimal TotalDebtAfterClosing { get; set; }

    /// <summary>Gets or sets the yearly interest rate, in percent.</summary>
    public decimal InterestRatePercent { get; set; }

    /// <summary>Gets or sets Fixed or Adjustable.</summary>
    public string RateType { get; set; } = string.Empty;

    /// <summary>Gets or sets the loan's term in years.</summary>
    public int TermYears { get; set; }

    /// <summary>Gets or sets the day the loan is expected to close.</summary>
    public DateOnly ExpectedClosingOn { get; set; }

    /// <summary>Gets or sets the business days from receipt to the expected closing.</summary>
    public int BusinessDaysBeforeClosing { get; set; }

    /// <summary>Gets or sets the loan's closing costs, if known.</summary>
    public decimal? ClosingCosts { get; set; }

    /// <summary>Gets or sets anything staff noted about the request.</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets the loan's appraised value, once recorded.</summary>
    public decimal? AppraisedValue { get; set; }

    /// <summary>Gets or sets the day of the loan's appraisal.</summary>
    public DateOnly? AppraisalDate { get; set; }

    /// <summary>Gets or sets the index value the appraisal used, when the lease's formula is index based.</summary>
    public decimal? CurrentIndexValue { get; set; }

    /// <summary>Gets or sets the purchase option price the appraisal gives.</summary>
    public decimal? PurchaseOptionPrice { get; set; }

    /// <summary>Gets or sets the lease's debt cap percent the check used.</summary>
    public int? MaxMortgageDebtPercent { get; set; }

    /// <summary>Gets or sets the most total debt may be.</summary>
    public decimal? DebtCapAmount { get; set; }

    /// <summary>Gets or sets the total debt as a percent of the purchase option price.</summary>
    public decimal? DebtPercent { get; set; }

    /// <summary>Gets or sets the day the trust approved or denied it, if it has.</summary>
    public DateOnly? DecidedOn { get; set; }

    /// <summary>Gets or sets why the trust approved or denied it.</summary>
    public string? DecisionReason { get; set; }

    /// <summary>Gets or sets whether the approved debt was over the cap.</summary>
    public bool? ApprovedOverCap { get; set; }

    /// <summary>Gets or sets the day the loan closed, if it has.</summary>
    public DateOnly? ClosedOn { get; set; }

    /// <summary>Gets or sets the permitted mortgage the closed loan became.</summary>
    public string? PermittedMortgageId { get; set; }

    /// <summary>Gets or sets the lease's mortgages the loan paid off.</summary>
    public List<string> PaidOffMortgageIds { get; set; } = [];

    /// <summary>Gets or sets the day the homeowner withdrew it, if they did.</summary>
    public DateOnly? WithdrawnOn { get; set; }

    /// <summary>Gets or sets why the homeowner withdrew it.</summary>
    public string? WithdrawnReason { get; set; }

    /// <summary>Gets or sets when the request was recorded.</summary>
    public DateTimeOffset RecordedAt { get; set; }
}
