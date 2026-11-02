namespace CommonGround.PropertyRegistry.Application.ReadModels;

/// <summary>Summary read model for home list displays.</summary>
public class HomeListItemView
{
    /// <summary>Gets or sets the unit ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the parent land parcel ID.</summary>
    public string LandParcelId { get; set; } = string.Empty;

    /// <summary>Gets or sets the unit designation (e.g. "Unit B"), if any.</summary>
    public string? UnitDesignation { get; set; }

    /// <summary>Gets or sets the unit type display name.</summary>
    public string UnitType { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of bedrooms.</summary>
    public int Bedrooms { get; set; }

    /// <summary>Gets or sets the number of bathrooms.</summary>
    public decimal Bathrooms { get; set; }

    /// <summary>Gets or sets the total square footage.</summary>
    public decimal SquareFootage { get; set; }

    /// <summary>Gets or sets the current assessed value.</summary>
    public decimal AssessedValue { get; set; }
}
