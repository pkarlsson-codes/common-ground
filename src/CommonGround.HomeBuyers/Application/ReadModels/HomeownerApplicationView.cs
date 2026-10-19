namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>Detail read model for a homeowner application.</summary>
public class HomeownerApplicationView
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the household unique identifier.</summary>
    public string HouseholdId { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the household size.</summary>
    public int HouseholdSize { get; set; }

    /// <summary>Gets or sets the annual household income.</summary>
    public decimal AnnualIncome { get; set; }

    /// <summary>Gets or sets whether accessibility accommodations are required.</summary>
    public bool AccessibilityRequired { get; set; }

    /// <summary>Gets or sets the current application phase.</summary>
    public string Phase { get; set; } = string.Empty;

    /// <summary>Gets or sets when the application was submitted.</summary>
    public DateTimeOffset SubmittedAt { get; set; }

    /// <summary>Gets or sets the user who submitted the application.</summary>
    public string SubmittedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets the approval date.</summary>
    public DateOnly? ApprovedDate { get; set; }
    
    /// <summary>Gets or sets the denial reason, if any.</summary>
    public string? DenialReason { get; set; }

    /// <summary>Gets or sets the withdrawal reason, if any.</summary>
    public string? WithdrawalReason { get; set; }

    /// <summary>Gets or sets when the applicant was selected.</summary>
    public DateOnly? SelectedDate { get; set; }

    /// <summary>Gets or sets the selected home identifier.</summary>
    public string? SelectedHomeId { get; set; }

    /// <summary>Gets or sets the selected land parcel identifier.</summary>
    public string? SelectedLandParcelId { get; set; }

    /// <summary>Gets or sets who selected the applicant.</summary>
    public string? SelectedBy { get; set; }
}