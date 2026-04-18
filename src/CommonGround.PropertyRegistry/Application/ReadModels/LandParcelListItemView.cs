namespace CommonGround.PropertyRegistry.Application.ReadModels;

/// <summary>Summary read model for land parcel list displays.</summary>
public class LandParcelListItemView
{
    /// <summary>Gets or sets the parcel ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the legal parcel number.</summary>
    public string LegalParcelNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the street address.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>Gets or sets the area in acres.</summary>
    public decimal AreaAcres { get; set; }

    /// <summary>Gets or sets the current status display name.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets when this parcel was registered.</summary>
    public DateTimeOffset RegisteredAt { get; set; }
}