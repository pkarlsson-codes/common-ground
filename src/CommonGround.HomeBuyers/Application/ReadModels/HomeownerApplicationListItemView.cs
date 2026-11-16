namespace CommonGround.HomeBuyers.Application.ReadModels;

public class HomeownerApplicationListItemView : IHomeownerApplicationPhaseView
{
    public string Id { get; set; } = string.Empty;

    public string HouseholdId { get; set; } = string.Empty;

    public int HouseholdSize { get; set; }

    public bool AccessibilityRequired { get; set; }

    public string Phase { get; set; } = string.Empty;

    public DateTimeOffset SubmittedAt { get; set; }

    /// <summary>Gets or sets the approval date; the household has been on the waiting list since then.</summary>
    public DateOnly? ApprovedDate { get; set; }

    /// <summary>Gets or sets the home the application is selected for, while it is selected or once purchased.</summary>
    public string? SelectedHomeId { get; set; }

    /// <summary>Gets or sets the date the application was selected for its home, while it is selected or once purchased.</summary>
    public DateOnly? SelectedDate { get; set; }

    /// <summary>Gets or sets the closing date of the purchase, once purchased.</summary>
    public DateOnly? PurchasedDate { get; set; }

    /// <summary>Gets or sets the date the application last closed (denied, withdrawn or removed from the waiting list), if it has.</summary>
    public DateOnly? ClosedDate { get; set; }
}