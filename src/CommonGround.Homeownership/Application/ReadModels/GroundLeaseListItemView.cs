namespace CommonGround.Homeownership.Application.ReadModels;

/// <summary>Summary read model for ground lease list views.</summary>
public class GroundLeaseListItemView
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

    /// <summary>Gets or sets the lease status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the current homeowner's full name.</summary>
    public string? HomeownerFullName { get; set; }

    /// <summary>Gets or sets the current homeowner's email.</summary>
    public string? HomeownerEmail { get; set; }

    /// <summary>Gets or sets when the lease was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
