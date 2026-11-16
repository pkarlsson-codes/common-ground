namespace CommonGround.Homeownership.Application.ReadModels;

public class MortgageRequestView
{
    public string Id { get; set; } = string.Empty;

    public string GroundLeaseId { get; set; } = string.Empty;

    public string HomeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the homeowner's full name when the request arrived.</summary>
    public string? HomeownerFullName { get; set; }

    /// <summary>Gets or sets Pending, Approved, Denied, Closed or Withdrawn.</summary>
    public string Status { get; set; } = string.Empty;

    public DateOnly ReceivedOn { get; set; }

    public string Lender { get; set; } = string.Empty;

    public string Purpose { get; set; } = string.Empty;

    public decimal Principal { get; set; }

    /// <summary>Gets or sets all mortgage debt on the home once the loan closes.</summary>
    public decimal TotalDebtAfterClosing { get; set; }

    /// <summary>Gets or sets the yearly interest rate, in percent.</summary>
    public decimal InterestRatePercent { get; set; }

    public string RateType { get; set; } = string.Empty;

    public int TermYears { get; set; }

    /// <summary>Gets or sets the day the loan is expected to close.</summary>
    public DateOnly ExpectedClosingOn { get; set; }

    /// <summary>Gets or sets the business days from receipt to the expected closing.</summary>
    public int BusinessDaysBeforeClosing { get; set; }

    /// <summary>Gets or sets the loan's closing costs, if known.</summary>
    public decimal? ClosingCosts { get; set; }

    public string? Notes { get; set; }

    /// <summary>Gets or sets the loan's appraised value, once recorded.</summary>
    public decimal? AppraisedValue { get; set; }

    public DateOnly? AppraisalDate { get; set; }

    /// <summary>Gets or sets the index value the appraisal used, when the lease's formula is index based.</summary>
    public decimal? CurrentIndexValue { get; set; }

    public decimal? PurchaseOptionPrice { get; set; }

    /// <summary>Gets or sets the lease's debt cap percent the check used.</summary>
    public int? MaxMortgageDebtPercent { get; set; }

    /// <summary>Gets or sets the most total debt may be.</summary>
    public decimal? DebtCapAmount { get; set; }

    /// <summary>Gets or sets the total debt as a percent of the purchase option price.</summary>
    public decimal? DebtPercent { get; set; }

    /// <summary>Gets or sets the day the trust approved or denied it, if it has.</summary>
    public DateOnly? DecidedOn { get; set; }

    public string? DecisionReason { get; set; }

    /// <summary>Gets or sets a value indicating whether the approved debt was over the cap.</summary>
    public bool? ApprovedOverCap { get; set; }

    /// <summary>Gets or sets the day the loan closed, if it has.</summary>
    public DateOnly? ClosedOn { get; set; }

    public string? PermittedMortgageId { get; set; }

    public List<string> PaidOffMortgageIds { get; set; } = [];

    /// <summary>Gets or sets the day the homeowner withdrew it, if they did.</summary>
    public DateOnly? WithdrawnOn { get; set; }

    public string? WithdrawnReason { get; set; }

    public DateTimeOffset RecordedAt { get; set; }
}
