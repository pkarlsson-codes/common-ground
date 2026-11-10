namespace CommonGround.Homeownership.Application.ReadModels;

/// <summary>Detailed read model for a single ground lease template.</summary>
public class GroundLeaseTemplateDetailView
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the template name.</summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>Gets or sets the lease term in years.</summary>
    public int TermYears { get; set; }

    /// <summary>Gets or sets the days the trust has to buy the home after its homeowner gives notice to sell.</summary>
    public int OptionWindowDays { get; set; }

    /// <summary>Gets or sets the monthly land use fee.</summary>
    public decimal LandUseFee { get; set; }

    /// <summary>Gets or sets the monthly repair reserve fee.</summary>
    public decimal RepairReserveFee { get; set; }

    /// <summary>Gets or sets the covenant terms text.</summary>
    public string CovenantTerms { get; set; } = string.Empty;

    /// <summary>Gets or sets the current status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the resale formula kind.</summary>
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

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the user ID of the creator.</summary>
    public string CreatedBy { get; set; } = string.Empty;
}
