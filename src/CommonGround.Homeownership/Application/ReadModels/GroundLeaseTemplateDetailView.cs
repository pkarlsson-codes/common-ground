namespace CommonGround.Homeownership.Application.ReadModels;

public class GroundLeaseTemplateDetailView
{
    public string Id { get; set; } = string.Empty;

    public string TemplateName { get; set; } = string.Empty;

    public int TermYears { get; set; }

    /// <summary>Gets or sets the days the trust has to buy the home after its homeowner gives notice to sell.</summary>
    public int OptionWindowDays { get; set; }

    /// <summary>Gets or sets the monthly land use fee.</summary>
    public decimal LandUseFee { get; set; }

    /// <summary>Gets or sets the monthly repair reserve fee.</summary>
    public decimal RepairReserveFee { get; set; }

    /// <summary>Gets or sets the yearly interest rate on a late fee, in percent; zero charges none.</summary>
    public decimal LateInterestRate { get; set; }

    /// <summary>Gets or sets the days after the due date within which a payment owes no interest.</summary>
    public int InterestWaiverDays { get; set; }

    /// <summary>Gets or sets the debt cap a lease from this template sets, a whole percent of the purchase option price.</summary>
    public int MaxMortgageDebtPercent { get; set; }

    /// <summary>Gets or sets the liability coverage per occurrence a lease from this template requires, or null for none.</summary>
    public decimal? LiabilityPerOccurrence { get; set; }

    public string CovenantTerms { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string FormulaKind { get; set; } = string.Empty;

    /// <summary>Gets or sets the appreciation share fraction (AppraisalBased only).</summary>
    public decimal? AppreciationShare { get; set; }

    /// <summary>Gets or sets the index type (IndexBased only).</summary>
    public string? IndexType { get; set; }

    /// <summary>Gets or sets the baseline index value (IndexBased only).</summary>
    public decimal? BaselineIndexValue { get; set; }

    /// <summary>Gets or sets the annual percentage (FixedPercentage only).</summary>
    public decimal? AnnualPercentage { get; set; }

    /// <summary>Gets or sets the improvements cap, if any.</summary>
    public decimal? ImprovementsCap { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
}
