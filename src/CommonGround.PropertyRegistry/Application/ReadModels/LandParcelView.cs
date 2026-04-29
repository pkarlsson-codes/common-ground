namespace CommonGround.PropertyRegistry.Application.ReadModels;

/// <summary>Detail read model for a land parcel.</summary>
public class LandParcelView
{
    /// <summary>Gets or sets the parcel ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the legal parcel number.</summary>
    public string LegalParcelNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the street address.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>Gets or sets the area in acres.</summary>
    public decimal AreaAcres { get; set; }

    /// <summary>Gets or sets the acquisition date.</summary>
    public DateOnly AcquisitionDate { get; set; }

    /// <summary>Gets or sets the acquisition method display name.</summary>
    public string AcquisitionMethod { get; set; } = string.Empty;

    /// <summary>Gets or sets optional covenant terms.</summary>
    public string? CovenantTerms { get; set; }

    /// <summary>Gets or sets whether this parcel is decommissioned.</summary>
    public bool IsDecommissioned { get; set; }

    /// <summary>Gets or sets when this parcel was registered.</summary>
    public DateTimeOffset RegisteredAt { get; set; }

    /// <summary>Gets or sets the ID of the user who registered this parcel.</summary>
    public string RegisteredBy { get; set; } = string.Empty;
}
