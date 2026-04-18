namespace CommonGround.PropertyRegistry.Application.ReadModels;

/// <summary>Summary read model for building list displays.</summary>
public class BuildingListItemView
{
    /// <summary>Gets or sets the building ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the parent land parcel ID.</summary>
    public string LandParcelId { get; set; } = string.Empty;

    /// <summary>Gets or sets the building type display name.</summary>
    public string BuildingType { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of bedrooms.</summary>
    public int Bedrooms { get; set; }

    /// <summary>Gets or sets the number of bathrooms.</summary>
    public decimal Bathrooms { get; set; }

    /// <summary>Gets or sets the total square footage.</summary>
    public decimal SquareFootage { get; set; }

    /// <summary>Gets or sets the current assessed value.</summary>
    public decimal AssessedValue { get; set; }

    /// <summary>Gets or sets the current status display name.</summary>
    public string Status { get; set; } = string.Empty;
}