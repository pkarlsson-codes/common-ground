namespace CommonGround.PropertyRegistry.Application.ReadModels;

public class HomeView
{
    public string Id { get; set; } = string.Empty;

    public string LandParcelId { get; set; } = string.Empty;

    /// <summary>Gets or sets the unit designation (e.g. "Unit B"), if any.</summary>
    public string? UnitDesignation { get; set; }

    public string UnitType { get; set; } = string.Empty;

    public int Bedrooms { get; set; }

    public decimal Bathrooms { get; set; }

    /// <summary>Gets or sets a value indicating whether the home is accessible to people with disabilities.</summary>
    public bool IsAccessible { get; set; }

    /// <summary>Gets or sets the income limit as a percent of AMI, or null when the home has no income limit.</summary>
    public int? IncomeLimitPercentOfAmi { get; set; }

    public decimal SquareFootage { get; set; }

    public int YearBuilt { get; set; }

    public decimal AssessedValue { get; set; }

    public DateOnly? LastAssessedDate { get; set; }

    public string? ConditionNotes { get; set; }

    public DateTimeOffset RegisteredAt { get; set; }
}
