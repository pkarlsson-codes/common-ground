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

    /// <summary>Gets or sets the latest appraisal's value, once one is recorded.</summary>
    public decimal? AppraisedValue { get; set; }

    /// <summary>Gets or sets the day of the latest appraisal.</summary>
    public DateOnly? AppraisalDate { get; set; }

    /// <summary>Gets or sets who made the latest appraisal.</summary>
    public string? AppraiserName { get; set; }

    /// <summary>Gets or sets the approved improvements the latest appraisal credited, if the lease's formula credits them.</summary>
    public decimal? ApprovedImprovements { get; set; }

    /// <summary>Gets or sets the current index value the latest appraisal used, if the lease's formula is index based.</summary>
    public decimal? CurrentIndexValue { get; set; }

    /// <summary>Gets or sets the most the lease's formula allows the home to sell for, as of the latest appraisal.</summary>
    public decimal? FormulaPrice { get; set; }

    /// <summary>Gets or sets what the trust pays to buy the home: the lesser of the formula price and the appraised value.</summary>
    public decimal? PurchaseOptionPrice { get; set; }

    /// <summary>Gets or sets the day the trust decided to buy the home, if it has.</summary>
    public DateOnly? OptionExercisedDate { get; set; }

    /// <summary>Gets or sets anything staff noted about the trust's decision.</summary>
    public string? OptionExercisedNotes { get; set; }

    /// <summary>Gets or sets the day the buyback closed, if it has.</summary>
    public DateOnly? ClosingDate { get; set; }

    /// <summary>Gets or sets what the trust paid at the closing.</summary>
    public decimal? PricePaid { get; set; }

    /// <summary>Gets or sets anything staff noted about the closing.</summary>
    public string? ClosingNotes { get; set; }

    /// <summary>Gets or sets whether the home was offered to the waiting list at the closing, rather than held.</summary>
    public bool? OfferedForSale { get; set; }

    /// <summary>Gets or sets why the trust is holding the home, if it is.</summary>
    public string? HoldReason { get; set; }

    /// <summary>Gets or sets the day the homeowner withdrew, if they did.</summary>
    public DateOnly? WithdrawnDate { get; set; }

    /// <summary>Gets or sets why the homeowner withdrew, if they did.</summary>
    public string? WithdrawnReason { get; set; }

    /// <summary>Gets or sets the user who recorded the notice.</summary>
    public string RecordedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets when the notice was recorded.</summary>
    public DateTimeOffset RecordedAt { get; set; }
}
