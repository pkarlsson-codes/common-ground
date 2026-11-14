namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>Summary read model for homeowner application list displays.</summary>
public class HomeownerApplicationListItemView : IHomeownerApplicationPhaseView
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the household id.</summary>
    public string HouseholdId { get; set; } = string.Empty;

    /// <summary>Gets or sets the household size.</summary>
    public int HouseholdSize { get; set; }

    /// <summary>Gets or sets whether accessibility accommodations are required.</summary>
    public bool AccessibilityRequired { get; set; }

    /// <summary>Gets or sets the current application phase.</summary>
    public string Phase { get; set; } = string.Empty;

    /// <summary>Gets or sets when the application was submitted.</summary>
    public DateTimeOffset SubmittedAt { get; set; }

    /// <summary>Gets or sets the approval date; the household has been on the waiting list since then.</summary>
    public DateOnly? ApprovedDate { get; set; }

    /// <summary>Gets or sets the home the application is selected for, while it is selected or once purchased.</summary>
    public string? SelectedHomeId { get; set; }

    /// <summary>Gets or sets the date the application was selected for its home, while it is selected or once purchased.</summary>
    public DateOnly? SelectedDate { get; set; }

    /// <summary>Gets or sets the closing date of the purchase, once purchased.</summary>
    public DateOnly? PurchasedDate { get; set; }

    /// <summary>Gets or sets the date the application last closed (denied or withdrawn), if it has.</summary>
    public DateOnly? ClosedDate { get; set; }
}