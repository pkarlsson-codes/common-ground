namespace CommonGround.HomeBuyers.Application.ReadModels;

public class HomeownerApplicationListItemView : IHomeownerApplicationPhaseView
{
    public string Id { get; set; } = string.Empty;

    public string HouseholdId { get; set; } = string.Empty;

    public int HouseholdSize { get; set; }

    public decimal AnnualIncome { get; set; }

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

    /// <summary>The day the household's income or size changed after approval, while staff haven't confirmed it still qualifies.</summary>
    public DateOnly? QualificationToConfirmSince { get; set; }

    /// <summary>While <see cref="QualificationToConfirmSince"/> is set, the income staff last checked.</summary>
    public decimal? QualifiedAnnualIncome { get; set; }

    /// <summary>While <see cref="QualificationToConfirmSince"/> is set, the household size staff last checked.</summary>
    public int? QualifiedHouseholdSize { get; set; }
}