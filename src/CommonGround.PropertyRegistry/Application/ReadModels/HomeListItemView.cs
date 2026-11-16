namespace CommonGround.PropertyRegistry.Application.ReadModels;

public class HomeListItemView
{
    public string Id { get; set; } = string.Empty;

    public string LandParcelId { get; set; } = string.Empty;

    /// <summary>Gets or sets the unit designation (e.g. "Unit B"), if any.</summary>
    public string? UnitDesignation { get; set; }

    public string UnitType { get; set; } = string.Empty;

    public int Bedrooms { get; set; }

    public decimal Bathrooms { get; set; }

    public decimal SquareFootage { get; set; }

    public decimal AssessedValue { get; set; }
}
