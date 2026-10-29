namespace CommonGround.PropertyRegistry.Application.ReadModels;

/// <summary>Detail read model for a home.</summary>
public class HomeView
{
    /// <summary>Gets or sets the unit ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the parent land parcel ID.</summary>
    public string LandParcelId { get; set; } = string.Empty;

    /// <summary>Gets or sets the unit type display name.</summary>
    public string UnitType { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of bedrooms.</summary>
    public int Bedrooms { get; set; }

    /// <summary>Gets or sets the number of bathrooms.</summary>
    public decimal Bathrooms { get; set; }

    /// <summary>Gets or sets a value indicating whether the home is accessible to people with disabilities.</summary>
    public bool IsAccessible { get; set; }

    /// <summary>Gets or sets the total square footage.</summary>
    public decimal SquareFootage { get; set; }

    /// <summary>Gets or sets the year built.</summary>
    public int YearBuilt { get; set; }

    /// <summary>Gets or sets the current assessed value.</summary>
    public decimal AssessedValue { get; set; }

    /// <summary>Gets or sets the date of the most recent assessment.</summary>
    public DateOnly? LastAssessedDate { get; set; }

    /// <summary>Gets or sets optional condition notes.</summary>
    public string? ConditionNotes { get; set; }

    /// <summary>Gets or sets the current status display name.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets when this unit was registered.</summary>
    public DateTimeOffset RegisteredAt { get; set; }
}
