namespace CommonGround.Homeownership.Application.ReadModels;

/// <summary>Read model for a resale, from the homeowner's notice of intent to sell.</summary>
public class ResaleView
{
    /// <summary>Gets or sets the resale ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the lease of the homeowner selling.</summary>
    public string GroundLeaseId { get; set; } = string.Empty;

    /// <summary>Gets or sets the leased land parcel.</summary>
    public string LandParcelId { get; set; } = string.Empty;

    /// <summary>Gets or sets the home being sold.</summary>
    public string HomeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the selling homeowner's full name.</summary>
    public string? HomeownerFullName { get; set; }

    /// <summary>Gets or sets the resale status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the day the homeowner gave notice.</summary>
    public DateOnly NoticeDate { get; set; }

    /// <summary>Gets or sets how the notice reached the trust.</summary>
    public string NoticeChannel { get; set; } = string.Empty;

    /// <summary>Gets or sets anything staff noted about the notice.</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets the last day of the trust's purchase option.</summary>
    public DateOnly OptionWindowEndsOn { get; set; }

    /// <summary>Gets or sets the day the homeowner withdrew, if they did.</summary>
    public DateOnly? WithdrawnDate { get; set; }

    /// <summary>Gets or sets why the homeowner withdrew, if they did.</summary>
    public string? WithdrawnReason { get; set; }

    /// <summary>Gets or sets the user who recorded the notice.</summary>
    public string RecordedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets when the notice was recorded.</summary>
    public DateTimeOffset RecordedAt { get; set; }
}
