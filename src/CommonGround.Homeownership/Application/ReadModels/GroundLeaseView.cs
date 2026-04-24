namespace CommonGround.Homeownership.Application.ReadModels;

/// <summary>Detail read model for a ground lease.</summary>
public class GroundLeaseView
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the land parcel ID.</summary>
    public string LandParcelId { get; set; } = string.Empty;

    /// <summary>Gets or sets the building ID.</summary>
    public string BuildingId { get; set; } = string.Empty;

    /// <summary>Gets or sets the lease start date.</summary>
    public DateOnly LeaseStartDate { get; set; }

    /// <summary>Gets or sets the lease term in years.</summary>
    public int TermYears { get; set; }

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
